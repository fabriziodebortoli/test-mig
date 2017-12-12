using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Microarea.Console.Core.SecurityLibrary;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.Lexan;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.SecurityLayer;
using Microarea.TaskBuilderNet.Core.StringLoader;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Interfaces;


namespace Microarea.Console.Plugin.SecurityAdmin
{
    #region Enumerativi
  

    public enum AllObjectsOperationType
    {
        ProtectAll = 0,
        UnprotectAll = 1,
        DeleteAllUserGrants = 2,
        RapidGrants = 3,
        GroupGrants = 4,
        DeleteAllRoleGrants = 5
    }

    //============================================================================
    public enum GrantOperationType
    {
        Allow = 4,
        Deny = 2
    }

      #endregion

   
    //=========================================================================
    public class NewObject
    {
        private bool isBatch = false;
        private bool isDataEntry = false;
        private bool isFinder = false;
        private bool isReport = false;

        protected string nameSpace;

        public string NameSpace { get { return nameSpace; } }

        public bool IsBatch { get { return isBatch; } }
        public bool IsDataEntry { get { return isDataEntry; } }
        public bool IsFinder { get { return isFinder; } }
        public bool IsReport { get { return isReport; } }

        //---------------------------------------------------------------------
        public NewObject(string aNamePace, bool aIsBatch, bool aIsDataEntry, bool aIsFinder, bool aIsReport)
        {
            nameSpace = aNamePace;

            isBatch = aIsBatch;
            isFinder = aIsFinder;
            isDataEntry = aIsDataEntry;
            isReport = aIsReport;
        }
    }

    //=========================================================================
	public class ImportExportFunction
	{


        static private int	objectId = -1;

		#region bit function
			//---------------------------------------------------------------------
			static public int SetDisable(int Flags)
		{
			if ((Flags & 8) != 0)
				return 1;
			else
				return 0;
		}
			//---------------------------------------------------------------------
			static public int SetAutentification(int Flags)
		{
			if ((Flags & 16) != 0)
				return 1;
			else
				return 0;

		}
        //---------------------------------------------------------------------
        #endregion

        #region Import / Export Data

        //---------------------------------------------------------------------
        static public bool DeleteFromProtectedObjects(int company, string connectionString)
        {

            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();

            SqlCommand deleteGrantsSqlCommand = null;

            try
            {

                string deleteGrantsQuery = @"delete from MSD_ProtectedObjects where ObjectId in (
                                                    select  ObjectId from MSD_ProtectedObjects
                                                    where  companyId = @CompanyId and FromGrant = 1 and not objectid in (select objectid from MSD_ObjectGrants where companyId = @CompanyId2))
                                                    and companyId = @CompanyId3 and FromGrant = 1 ";

                deleteGrantsSqlCommand = new SqlCommand(deleteGrantsQuery, conn);

                deleteGrantsSqlCommand.Parameters.AddWithValue("@CompanyId", company);
                deleteGrantsSqlCommand.Parameters.AddWithValue("@CompanyId2", company);
                deleteGrantsSqlCommand.Parameters.AddWithValue("@CompanyId3", company);

                int i = deleteGrantsSqlCommand.ExecuteNonQuery();

                return true;
            }
            catch (SqlException err)
            {
                DiagnosticViewer.ShowError(Strings.Error, err.Message, err.Procedure, err.Number.ToString(), SecurityConstString.SecurityAdminPlugIn);

                return false;
            }
            finally
            {
                if (deleteGrantsSqlCommand != null)
                    deleteGrantsSqlCommand.Dispose();
            }
        }

        //--------------------------------------------------------------------------------------------------------------
        static public bool ExportDBObjectsInXML(SqlConnection sqlOSLConnection, XmlDocument xmlDocument, XmlElement rolesElement, string connectionString)
        {
            SqlCommand cmd		= new SqlCommand();
			SqlDataReader read	= null;

			try
			{
				string sSelect =@"select MSD_Objects.*, MSD_ObjectTypes.Type from MSD_Objects, MSD_ObjectTypes
                                    where 
                                  MSD_Objects.TypeId = MSD_ObjectTypes.TypeId AND ParentId <> 0 order by objectid";
                			
				cmd.Connection	= sqlOSLConnection;
				cmd.CommandText = sSelect;
                read = cmd.ExecuteReader();

                if (rolesElement == null)
                    return false;

                XmlElement objectsElement = xmlDocument.CreateElement("Objects");
                rolesElement.AppendChild(objectsElement);

				
				XmlElement objectElement = null;
				XmlElement namespaceElement = null;
				XmlElement typeIDElement = null;

				while ( read.Read())
				{
                    objectElement = xmlDocument.CreateElement("Object");
                    if (objectElement != null)
                    {
                        namespaceElement = xmlDocument.CreateElement("NameSpace");

                        if (namespaceElement != null)
                            namespaceElement.InnerText = read["NameSpace"].ToString();

                        objectElement.AppendChild(namespaceElement);

                        typeIDElement = xmlDocument.CreateElement("TypeID");

                        if (typeIDElement != null)
                            typeIDElement.InnerText = read["type"].ToString();

                        objectElement.AppendChild(typeIDElement);
                        
                        //A questo punto cerco i dati del papa' 
                        ExtractParentData(connectionString, Convert.ToInt32(read["ParentId"]), objectElement, xmlDocument);
                        objectsElement.AppendChild(objectElement);
                    }
                }

                read.Close();
                cmd.Dispose();

                return true;
            }


            catch (SqlException)
            {
                return false;
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                if (read != null && !read.IsClosed)
                    read.Close();
                if (cmd != null)
                    cmd.Dispose();
            }
        
        }
        //---------------------------------------------------------------------
        static private void ExtractParentData(string connectionString, int objectId, XmlElement parentNode, XmlDocument xmlDocument)
            {
                string sSelect = @"select MSD_Objects.*, MSD_ObjectTypes.Type from MSD_Objects, MSD_ObjectTypes
                                    where 
                                    MSD_Objects.TypeId = MSD_ObjectTypes.TypeId AND objectId = " + objectId;

                SqlConnection conn = new SqlConnection(connectionString);
                SqlCommand cmd = new SqlCommand(sSelect, conn);
                SqlDataReader reader = null;
                
                XmlElement namespaceParentElement = null;
                XmlElement typeIdParentElement = null;

                try
                {
                    conn.Open();
                    reader = cmd.ExecuteReader();

                    reader.Read();
                  
                    namespaceParentElement = xmlDocument.CreateElement("NameSpaceParent");
                    if (namespaceParentElement != null)
                    {
                        namespaceParentElement.InnerText = reader["NameSpace"].ToString();
                        parentNode.AppendChild(namespaceParentElement);
                    }

                    typeIdParentElement = xmlDocument.CreateElement("TypeIDParent");
                    if (typeIdParentElement != null)
                    {
                        typeIdParentElement.InnerText = reader["Type"].ToString();
                        parentNode.AppendChild(typeIdParentElement);
                    }
                  

                    reader.Close();
                    cmd.Dispose();
                    conn.Close();
                }
                catch (SqlException ex)
                {
                    string a = ex.Message;
                }
                catch (Exception ex)
                {
                    string a = ex.Message;
                }

                finally
                {
                    if (reader != null && !reader.IsClosed)
                        reader.Close();
                    if (cmd != null)
                        cmd.Dispose();
                    if (conn != null && conn.State != ConnectionState.Closed)
                        conn.Close();
                }

            }
		//--------------------------------------------------------------------------------------------------------------
		static public bool ExportRolesInXML(int companyId, string fileName, ArrayList roleIdsList, SqlConnection sqlOSLConnection, string connectionString)
		{
			if (roleIdsList == null || roleIdsList.Count == 0 || sqlOSLConnection == null || sqlOSLConnection.State != ConnectionState.Open)
				return false;

            XmlDocument xmlDocument =  new XmlDocument();
			XmlDeclaration declaration = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);

			if (declaration != null)
			    xmlDocument.AppendChild(declaration);

			XmlElement rolesElement = xmlDocument.CreateElement("Roles");
			if (rolesElement != null)
				xmlDocument.AppendChild(rolesElement);

            ExportDBObjectsInXML(sqlOSLConnection, xmlDocument, rolesElement, connectionString);

			SqlCommand cmd		= new SqlCommand();
			SqlDataReader read	= null;

			try
			{
				string sSelect =@"SELECT * FROM MSD_CompanyRoles WHERE CompanyId = @CompanyId AND RoleId = @RoleId and readonly= 0";

                cmd.Parameters.AddWithValue("@CompanyId", companyId);
                cmd.Parameters.AddWithValue("@RoleId", Convert.ToInt32(roleIdsList[0]));
				
				//ATTACCO I RUOLI DA ESPORTARE e compongo il commandText
				for(int i=1; i < roleIdsList.Count; i++)
				{
					sSelect = sSelect + " OR "; //Attacco l'end
					sSelect = sSelect + " RoleId = @" + i.ToString();
                    cmd.Parameters.AddWithValue("@" + i.ToString(), Convert.ToInt32(roleIdsList[i]));
				}
				
				cmd.Connection	= sqlOSLConnection;
				cmd.CommandText = sSelect;

				read = cmd.ExecuteReader();



				XmlElement roleElement = null;
				XmlElement disabledElement = null;
				XmlElement descriptionElement = null;
                XmlElement readOnlyElement = null;

				while ( read.Read())
				{
					roleElement = xmlDocument.CreateElement("Role");
					if (roleElement != null)
					{
						roleElement.SetAttribute("localize", read["Role"].ToString());

						disabledElement = xmlDocument.CreateElement("Disabled");
						if (disabledElement != null)
						{
							disabledElement.InnerText = read["Disabled"].ToString();
						}	
						roleElement.AppendChild(disabledElement);

                        readOnlyElement =  xmlDocument.CreateElement("ReadOnly");
                        if (readOnlyElement != null)
						{
                            readOnlyElement.InnerText = read["ReadOnly"].ToString();
						}
                        roleElement.AppendChild(readOnlyElement);

						descriptionElement = xmlDocument.CreateElement("Description");
						if (descriptionElement != null)
						{
							descriptionElement.InnerText = read["Description"].ToString();
						}	
						descriptionElement.SetAttribute("localizable", "true");
						roleElement.AppendChild(descriptionElement);
						//Qui dovrei attaccare l'altro loop
						ExtractGrantsForRole(connectionString, Convert.ToInt32(read["RoleId"]), ref roleElement, Convert.ToInt32(read["CompanyId"]), xmlDocument);
						rolesElement.AppendChild(roleElement);
					}
				}
				
				read.Close();
				cmd.Dispose();

				if (xmlDocument != null)
					xmlDocument.Save(fileName);

				return true;
			}

		
			catch(SqlException)
			{
				return false;
			}
			catch(Exception err)
			{
				MessageBox.Show(err.Message , "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
			finally
			{
				if (read != null && !read.IsClosed) 
					read.Close();
				if (cmd != null)
					cmd.Dispose();
			}
		}
        //--------------------------------------------------------------------------------------------------------------
        static private ArrayList GetGrantsMaskByObjectTypeId(int objectTypeId, SqlConnection sqlOSLConnection)
        {
            if (objectTypeId == -1 || objectTypeId == 0 || sqlOSLConnection == null || sqlOSLConnection.State != ConnectionState.Open)
                return null;

            SqlCommand cmd = null; new SqlCommand();
            SqlDataReader reader = null;

            string sSelect = "Select GrantMask from MSD_ObjectTypeGrants where TypeId =" + objectTypeId;

            ArrayList grants = new ArrayList();
            try
            {
                cmd = new SqlCommand(sSelect, sqlOSLConnection);
                reader = cmd.ExecuteReader();

                while (reader.Read())
                    grants.Add(Convert.ToInt32(reader["GrantMask"]));

                reader.Close();
                cmd.Dispose();

                return grants;
            }
            catch (SqlException ex)
            {
                string a = ex.Message;
            }
            catch (Exception ex)
            {
                string a = ex.Message;
            }

            finally
            {
                if (reader != null && !reader.IsClosed)
                    reader.Close();
                if (cmd != null)
                    cmd.Dispose();
            }
            return null;

        }
		//---------------------------------------------------------------------
		static private void ExtractGrantsForRole(string connectionString, int roleId, ref XmlElement parentNode, int companyId, XmlDocument xmlDocument )
		{
		
            //Cosi'estraggo i primi livelli
			string sSelect = @"SELECT MSD_ObjectGrants.Grants, 
							MSD_ObjectGrants.InheritMask, 
							MSD_Objects.NameSpace, 
							MSD_ObjectTypes.Type,
							MSD_CompanyRoles.RoleId,
							MSD_CompanyRoles.Role,
							MSD_Objects.ParentId,
                            MSD_Objects.ObjectID
							FROM MSD_ObjectGrants 
						INNER JOIN MSD_Objects ON MSD_Objects.ObjectId = MSD_ObjectGrants.ObjectId 
						INNER JOIN MSD_CompanyRoles ON MSD_CompanyRoles.RoleId = MSD_ObjectGrants.RoleId
						INNER JOIN MSD_ObjectTypes ON MSD_ObjectTypes.TypeId = MSD_Objects.TypeId
						WHERE MSD_CompanyRoles.CompanyId = @CompanyId AND MSD_CompanyRoles.RoleId = @RoleId
                        order  by Msd_objects.objectid";

			SqlConnection conn = new SqlConnection(connectionString);
			SqlCommand cmd = new SqlCommand(sSelect, conn);
			SqlDataReader reader = null;

            cmd.Parameters.AddWithValue("@CompanyId", companyId);
            cmd.Parameters.AddWithValue("@RoleId", roleId);

			XmlElement grantElement = null;
			XmlElement namespaceElement = null;
			XmlElement typeIdElement = null;
			XmlElement grantValueElement = null;
			XmlElement inheritmaskElement = null;

			try
			{
				conn.Open();
				reader = cmd.ExecuteReader();

				XmlElement grantsElement = xmlDocument.CreateElement("Grants");
				if (grantsElement != null)
					parentNode.AppendChild(grantsElement);
				
				while (reader.Read())
				{
					grantElement = xmlDocument.CreateElement("Grant");
					if (grantElement != null)
						grantsElement.AppendChild(grantElement);


					namespaceElement = xmlDocument.CreateElement("NameSpace");
					if (namespaceElement != null)
					{
						namespaceElement.InnerText = reader["NameSpace"].ToString();
						grantElement.AppendChild(namespaceElement);
					}
					
					typeIdElement = xmlDocument.CreateElement("TypeId");
					if (typeIdElement != null)
					{
						typeIdElement.InnerText = reader["Type"].ToString();
						grantElement.AppendChild(typeIdElement);
					}

					grantValueElement = xmlDocument.CreateElement("GrantValue");
					if (grantValueElement != null)
					{
						grantValueElement.InnerText = reader["Grants"].ToString();
						grantElement.AppendChild(grantValueElement);
					}

					inheritmaskElement = xmlDocument.CreateElement("InheritMask");
					if (inheritmaskElement != null)
					{
						inheritmaskElement.InnerText = reader["InheritMask"].ToString();
						grantElement.AppendChild(inheritmaskElement);
					}
					
                    //Cerco i suoi figli tramite la ricorsiva
                   // ExportParentObjects(Convert.ToInt32(reader["ObjectID"].ToString()), grantElement, connectionString, xmlDocument, roleId, companyId);
				}

				reader.Close();
				cmd.Dispose();
				conn.Close();
			}
			catch(SqlException ex)
			{
				string a = ex.Message;
			}
			catch(Exception ex)
			{
				string a = ex.Message;
			}

			finally
			{
				if (reader != null && !reader.IsClosed) 
					reader.Close();
				if (cmd != null)
					cmd.Dispose();
				if (conn != null && conn.State != ConnectionState.Closed)
					conn.Close();
			}

		}

		//---------------------------------------------------------------------
		static private bool ExportParentObjects(int parentId, XmlElement grantsElement, string connectionString, XmlDocument xmlDocument, int aroleid, int companyID)
		{
			if (parentId== 0 )
				return false;
            int roleid = aroleid;

			if (grantsElement == null)
				return false;

			string select = @"SELECT MSD_ObjectGrants.Grants, 
							MSD_ObjectGrants.InheritMask, 
							MSD_Objects.NameSpace, 
							MSD_ObjectTypes.Type,
							MSD_CompanyRoles.RoleId,
							MSD_CompanyRoles.Role,
							MSD_Objects.ParentId,
                            MSD_Objects.ObjectID
							FROM MSD_ObjectGrants 
						INNER JOIN MSD_Objects ON MSD_Objects.ObjectId = MSD_ObjectGrants.ObjectId 
						INNER JOIN MSD_CompanyRoles ON MSD_CompanyRoles.RoleId = MSD_ObjectGrants.RoleId
						INNER JOIN MSD_ObjectTypes ON MSD_ObjectTypes.TypeId = MSD_Objects.TypeId
						WHERE MSD_CompanyRoles.CompanyId = @CompanyId AND MSD_CompanyRoles.RoleId = @RoleId and 
                              MSD_Objects.ParentId = @parentId";

            XmlElement parentElement = null;
            XmlElement namespaceElement = null;
            XmlElement typeIdElement = null;
            XmlElement grantValueElement = null;
            XmlElement inheritmaskElement = null;
            XmlElement grantElement = null;

			SqlConnection sqlConnection = new SqlConnection(connectionString);
			sqlConnection.Open();

			SqlCommand command	= new SqlCommand(select, sqlConnection);
            command.Parameters.AddWithValue("@CompanyId", companyID);
            command.Parameters.AddWithValue("@RoleId", aroleid);
            command.Parameters.AddWithValue("@parentId", parentId);

			SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                grantElement = xmlDocument.CreateElement("Grant");
                if (grantElement != null)
                    grantsElement.AppendChild(grantElement);


                namespaceElement = xmlDocument.CreateElement("NameSpace");
                if (namespaceElement != null)
                {
                    namespaceElement.InnerText = reader["NameSpace"].ToString();
                    grantElement.AppendChild(namespaceElement);
                }

                typeIdElement = xmlDocument.CreateElement("TypeId");
                if (typeIdElement != null)
                {
                    typeIdElement.InnerText = reader["Type"].ToString();
                    grantElement.AppendChild(typeIdElement);
                }

                grantValueElement = xmlDocument.CreateElement("GrantValue");
                if (grantValueElement != null)
                {
                    grantValueElement.InnerText = reader["Grants"].ToString();
                    grantElement.AppendChild(grantValueElement);
                }

                inheritmaskElement = xmlDocument.CreateElement("InheritMask");
                if (inheritmaskElement != null)
                {
                    inheritmaskElement.InnerText = reader["InheritMask"].ToString();
                    grantElement.AppendChild(inheritmaskElement);
                }

                parentElement = xmlDocument.CreateElement("ParentId");
                if (parentElement != null)
                {
                    parentElement.InnerText = reader["ParentId"].ToString();
                    grantElement.AppendChild(parentElement);
                }

                ExportParentObjects(Convert.ToInt32(reader["ObjectID"]), grantElement, connectionString, xmlDocument, roleid, companyID);
            }

			return true;


		}
        //---------------------------------------------------------------------
        private static int InsertRoleInDB(Role role, SqlConnection sqlOSLConnection, int companyId)
        {
            if (sqlOSLConnection == null || sqlOSLConnection.State != ConnectionState.Open)
                return -1;

            if (role == null)
                return -1;


            //Inserisco i Ruoli
            string sInsert = @"INSERT INTO MSD_CompanyRoles 
					(CompanyId, Role, Description, LastModifyGrants)
					VALUES
					(@CompanyId, @Role, @Description, @LastModifyGrants)";

            SqlCommand mySQLCommand = null;

            try
            {
                mySQLCommand = new SqlCommand(sInsert, sqlOSLConnection);

                mySQLCommand.Parameters.AddWithValue("@CompanyId", companyId);
                mySQLCommand.Parameters.AddWithValue("@Role", role.RoleNewName);
                mySQLCommand.Parameters.AddWithValue("@Description", role.RoleDescription);
                mySQLCommand.Parameters.AddWithValue("@LastModifyGrants", DateTime.Now);

                mySQLCommand.ExecuteNonQuery();

                mySQLCommand.Dispose();


                return GetRoleId(role.RoleName, companyId, sqlOSLConnection);
            }

            catch (SqlException)
            {
            }
            finally
            {
                if (mySQLCommand != null)
                    mySQLCommand.Dispose();
            }

            return -1;

        }

        //---------------------------------------------------------------------------
        public static string GetReportDefaultSecurityRoles(string reportFileName)
        {
            if (reportFileName == null || reportFileName.Length == 0 || !File.Exists(reportFileName))
                return String.Empty;

            Parser reportParser = new Parser(Parser.SourceType.FromFile);
            if (!reportParser.Open(reportFileName))
                return String.Empty;

            string defaultSecurityRoles = String.Empty;
            if (reportParser.SkipToToken(Token.DEFAULTSECURITYROLES, true, false))
                reportParser.ParseString(out defaultSecurityRoles);

            reportParser.Close();

            if (defaultSecurityRoles == null || defaultSecurityRoles.Length == 0)
                return String.Empty;

            return defaultSecurityRoles;
        }

        //---------------------------------------------------------------------
        static public bool ImportBaseRolesGrantFromXML(ArrayList roles, SqlConnection sqlOSLConnection, int companyId, PathFinder pathfinder, string connectionString)
        {
            if (sqlOSLConnection == null || sqlOSLConnection.State != ConnectionState.Open)
                return false;

            if (roles == null || roles.Count == 0)
                return false;

            if (!ExistObjectsInDB(sqlOSLConnection))
            {
                DiagnosticViewer.ShowWarning(Strings.NoInsertObject, SecurityConstString.SecurityAdminPlugIn);
                return false;
            }

            //Inserisco i ruoli nel DB
            foreach (Role role in roles)
            {
                int roleId = InsertRoleInDB(role, sqlOSLConnection, companyId);
                if (roleId != -1)
                    role.RoleId = roleId;

            }

            foreach (BaseApplicationInfo app in pathfinder.ApplicationInfos)
            {

                if (app.Modules == null || app.Modules.Count == 0)
                    continue;

                foreach (BaseModuleInfo mod in app.Modules)
                {

                    if (mod.DocumentObjectsInfo != null &&
                        mod.DocumentObjectsInfo.Documents != null &&
                        mod.DocumentObjectsInfo.Documents.Count > 0)
                    {
                        //Per ogni modulo cerco i DocumentObjects.xml
                        foreach (DocumentInfo doc in mod.DocumentObjectsInfo.Documents)
                        {

                            if (doc.IsSecurityhidden)
                                continue;

                            if (doc.DefaultSecurityRoles == null || doc.DefaultSecurityRoles.Length == 0)
                                continue;

                            //Controllo i ruoli associati
                            if (!DefaultSecurityRolesEngine.AreValidRoles(doc.DefaultSecurityRoles))
                                continue;

                            string[] rolesName = doc.DefaultSecurityRoles.Split(',');


                            int objectType = GetTypeFromNewObject(doc.IsBatch, doc.IsDataEntry, doc.IsFinder, false);
                            int objectTypeId = CommonObjectTreeFunction.GetObjectTypeId(objectType, sqlOSLConnection);

                            InsertGrant(rolesName, roles, objectType, objectTypeId, doc.NameSpace.GetNameSpaceWithoutType(), companyId, sqlOSLConnection, connectionString);
                        }
                    }

                    string reportPath = Path.Combine(mod.Path, "Report");

                    if (Directory.Exists(reportPath))
                    {

                        string[] wrmFiles = Directory.GetFiles(reportPath, "*.wrm");

                        foreach (string wrmFile in wrmFiles)
                        {
                            string reportName = wrmFile.Substring(wrmFile.LastIndexOf('\\') + 1);

                            reportName = reportName.Substring(0, reportName.IndexOf("."));

                            //Tolgo i blank e faccio i raplace con degli _
                            reportName = reportName.Trim();
                            reportName = reportName.Replace(" ", "_");

                            string reportNameSpace = String.Concat(app.Name, ".", mod.Name, ".", reportName);

                            string rolesName = GetReportDefaultSecurityRoles(wrmFile);
                            string[] roleNameArray = rolesName.Split(',');

                            if (roles == null || roles.Count == 0 || String.IsNullOrEmpty(rolesName))
                                continue;

                            int objectType = 4;
                            int objectTypeId = CommonObjectTreeFunction.GetObjectTypeId(objectType, sqlOSLConnection);

                            InsertGrant(roleNameArray, roles, objectType, objectTypeId, reportNameSpace, companyId, sqlOSLConnection, connectionString);
                        }
                    }

                    if (mod.WebMethods == null || mod.WebMethods.Count == 0)
                        continue;

                    foreach (FunctionPrototype functionInfo in mod.WebMethods)
                    {
                        if (functionInfo.IsSecurityhidden)
                            continue;

                        if (functionInfo.DefaultSecurityRoles != string.Empty && functionInfo.DefaultSecurityRoles != null)
                        {
                            int objectType = 3;
                            int objectTypeId = CommonObjectTreeFunction.GetObjectTypeId(objectType, sqlOSLConnection);
                            InsertGrant(functionInfo.DefaultSecurityRoles.Split(','), roles, objectType, objectTypeId, functionInfo.NameSpace.GetNameSpaceWithoutType(), companyId, sqlOSLConnection, connectionString);
                        }
                    }
                }
            }

            return true;
        }

        //---------------------------------------------------------------------
        static private bool ExistGrantsForRole(string connectionString, int roleId, int objectId, int companyId)
        {
            string sSelect = @"SELECT count(*) FROM MSD_ObjectGrants 
							    WHERE CompanyId = @CompanyId AND RoleId = @RoleId and objectid = @objectId";

            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand(sSelect, conn);

            cmd.Parameters.AddWithValue("@CompanyId", companyId);
            cmd.Parameters.AddWithValue("@RoleId", roleId);
            cmd.Parameters.AddWithValue("@objectId", objectId);

            try
            {
                conn.Open();
                return ((int)cmd.ExecuteScalar() > 0);
            }
            catch (SqlException ex)
            {
                string a = ex.Message;
                return false;
            }
            catch (Exception ex)
            {
                string a = ex.Message;
                return false;
            }

            finally
            {
                if (cmd != null)
                    cmd.Dispose();
            }

        }
        //---------------------------------------------------------------------
        static public bool InsertGrants(int grant, int inheritMask, SqlConnection aSqlOSLConnection, int companyId, int objectId, int roleId, string connectionString)
        {
            if (aSqlOSLConnection == null || aSqlOSLConnection.State != ConnectionState.Open || companyId == -1)
                return false;

            if (ExistGrantsForRole(connectionString, roleId, objectId, companyId))
                return false;

            SqlCommand insertRoleCommand = null;

            try
            {
				//Ora che ho il codice del Ruolo posso inserire la riga nella ObjectGrants
                string sInsert = @"INSERT INTO MSD_ObjectGrants
								(CompanyId, ObjectId, LoginId, RoleId, Grants, InheritMask)
								VALUES
								(@CompanyId, @ObjectId, 0, @RoleId, @Grants, @InheritMask)";

                insertRoleCommand = new SqlCommand(sInsert, aSqlOSLConnection);
                insertRoleCommand.Parameters.AddWithValue("@CompanyId", companyId);
                insertRoleCommand.Parameters.AddWithValue("@ObjectId", objectId);
                insertRoleCommand.Parameters.AddWithValue("@RoleId", roleId);
                insertRoleCommand.Parameters.AddWithValue("@Grants", grant.ToString());
                insertRoleCommand.Parameters.AddWithValue("@InheritMask", inheritMask.ToString());

                insertRoleCommand.ExecuteNonQuery();

                return true;
            }
            catch (Exception e)
            {
                Debug.Fail("Exception thrown in CommonObjectTreeFunction.InsertGrants: " + e.Message);

                return false;
            }
            finally
            {
                if (insertRoleCommand != null)
                    insertRoleCommand.Dispose();
            }
        }

        //---------------------------------------------------------------------
        static public bool InsertGrants(Grant grantsRow, SqlConnection aSqlOSLConnection, int companyId, int objectId, string roleName, string connectionString )
        {
            if (aSqlOSLConnection == null || companyId == -1)
                return false;

            int roleId = GetRoleId(roleName, companyId, aSqlOSLConnection);
            if (roleId == -1)
                return false; //Non ho trovato il ruolo!!!


            return InsertGrants(grantsRow.GrantValue, grantsRow.InheritMask, aSqlOSLConnection, companyId, objectId, roleId, connectionString);
        }

        //---------------------------------------------------------------------
        private static void InsertGrant(string[] rolesName, ArrayList roles, int objectType, int objectTypeId, string nameSpace, int companyId, SqlConnection sqlOSLConnection, string connectionString)
        {
            foreach (string roleFromXml in rolesName)
            {
                foreach (Role role in roles)
                {
                    if (string.Compare(role.RoleName, roleFromXml) == 0 ||
                        string.Compare(DefaultAdvancedRoles.GetBaseRoleFromAdvancedRole(role.RoleName), roleFromXml) == 0)
                    {

                        //Controllo se l'oggetto è nel DB
                        if (!ExistObject(nameSpace, objectTypeId, sqlOSLConnection))
                            CommonObjectTreeFunction.InsertObjectInDB(nameSpace, objectType, sqlOSLConnection);

                        //prendo l'objectid
                        int objectId = CommonObjectTreeFunction.GetObjectId(nameSpace, objectTypeId, sqlOSLConnection);
                        if (objectId == -1)
                            continue;

                        //Se non é protetto lo proteggo
                        if (!IsProtected(nameSpace, objectTypeId, companyId, sqlOSLConnection))
                            CommonObjectTreeFunction.ProtectObject(companyId, objectId, sqlOSLConnection, true);

                        ArrayList gratsMask = GetGrantsMaskByObjectTypeId(objectTypeId, sqlOSLConnection);
                        if (gratsMask == null || gratsMask.Count == 0)
                            continue;

                        int grant = SetGrantByDefaultRoleType(gratsMask, role.RoleType, objectType);

                        InsertGrants(grant, 0, sqlOSLConnection, companyId, objectId, role.RoleId, connectionString);
                    }
                }
            }
        }

        //---------------------------------------------------------------------
        private static int SetGrantByDefaultRoleType(ArrayList gratsMask, DefaultAdvancedRolesType roleType, int objectType)
        {
            int grant = 0;

            if (roleType == DefaultAdvancedRolesType.Director || roleType == DefaultAdvancedRolesType.Base)
            {
                foreach (int mask in gratsMask)
                    grant = Bit.SetUno(grant, mask);
            }

            if (roleType == DefaultAdvancedRolesType.Assistant)
                grant = SetGrantForAssistant(gratsMask, objectType);

            if (roleType == DefaultAdvancedRolesType.Employee)
                grant = SetGrantForEmployee(gratsMask, objectType);

            return grant;
        }

        //---------------------------------------------------------------------
        private static int SetGrantForAssistant(ArrayList gratsMask, int objectType)
        {
            int grant = 0;

            foreach (int mask in gratsMask)
            {
                //Se é un report e ilgrant é modifica lo nego
                if ((objectType == 4 && mask == 2) || mask == 32 || mask == 64)
                    grant = Bit.SetZero(grant, mask);
                else
                    grant = Bit.SetUno(grant, mask);
            }

            return grant;
        }

        //---------------------------------------------------------------------
        private static int SetGrantForEmployee(ArrayList gratsMask, int objectType)
        {
            int grant = 0;

            foreach (int mask in gratsMask)
            {
                //Se é un report e ilgrant é modifica lo nego
                if ((objectType == 4 && mask == 2) || mask == 32 || mask == 64 || mask == 128 || mask == 1024 || (objectType == 4 && mask == 256))
                    grant = Bit.SetZero(grant, mask);
                else
                    grant = Bit.SetUno(grant, mask);
            }

            return grant;
        }

        //---------------------------------------------------------------------
        private static string GetTypeNameFromNewObject(NewObject obj)
        {
            if (obj.IsBatch)
                return "Batch";

            if (obj.IsFinder)
                return "Finder";

            if (obj.IsReport)
                return "Report";

            return "Data Entry";
        }

        //---------------------------------------------------------------------
        private static int GetTypeFromNewObject(bool isBatch, bool isDataEntry, bool isFinder, bool isReport)
        {
            if (isBatch)
                return 7;

            if (isFinder)
                return 21;

            if (isReport)
                return 4;

            return 5;
        }

        //---------------------------------------------------------------------
        static public bool ImportDBObjects(ArrayList objects, SqlConnection sqlOSLConnection) 
        {
            if (sqlOSLConnection == null || sqlOSLConnection.State != ConnectionState.Open)
                return false;
            
            if (!ExistObjectsInDB(sqlOSLConnection))
            {
                DiagnosticViewer.ShowWarning(Strings.NoInsertObject, SecurityConstString.SecurityAdminPlugIn);
                return false;
            }

            foreach (ObjectForGrantsImport obj in objects)
            {
                if (obj.NameSpace == string.Empty || obj.TypeId == 0)
                    continue;

                //Per prima cosa guardo se esiste gia' 
                if (!ExistObject(obj.NameSpace, CommonObjectTreeFunction.GetObjectTypeId(obj.TypeId, sqlOSLConnection), sqlOSLConnection))
                { 
                    //Non esiste quindi devi inserirlo ma prima controlliamo se esiste il parent
                    if (!ExistObject(obj.NameSpaceParent, CommonObjectTreeFunction.GetObjectTypeId(obj.TypeIdParent, sqlOSLConnection), sqlOSLConnection))
                        CommonObjectTreeFunction.InsertObjectInDB(new NameSpace(obj.NameSpaceParent),obj.TypeIdParent, sqlOSLConnection);
                 
                    int parentId = CommonObjectTreeFunction.GetObjectId(obj.NameSpaceParent, CommonObjectTreeFunction.GetObjectTypeId(obj.TypeIdParent, sqlOSLConnection), sqlOSLConnection);
                    if (parentId == -1)
                        continue;

                    CommonObjectTreeFunction.InsertObjectInDB(new NameSpace(obj.NameSpace), obj.TypeId, sqlOSLConnection, parentId);
                }
            }

            return true;
        }

		//---------------------------------------------------------------------
        static public bool ImportRolesFromXML(Role role, SqlConnection sqlOSLConnection, int companyId, string connectionString, bool isOFM)
		{
			if (sqlOSLConnection == null || sqlOSLConnection.State != ConnectionState.Open)
				return false;

			if (role == null)
				return false;

            int roleid = GetIdRole(companyId.ToString(), role.RoleName, connectionString) ;
            if (!isOFM)
            {
                //Per prima cosa devo controllare se esiste gia un ruolo con quel id
                if (roleid != -1)
                {
                    if (MessageBox.Show(Strings.RoleAreadyExist, Strings.Warning, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                        return false;
                }
                else
                {
                    //Inserisco i Ruoli
                    string sInsert = @"INSERT INTO MSD_CompanyRoles 
						(CompanyId, Role, Description, LastModifyGrants, ReadOnly)
					  VALUES
						(@CompanyId, @Role, @Description, @LastModifyGrants, @ReadOnly)";

                    SqlCommand mySQLCommand = new SqlCommand(sInsert, sqlOSLConnection);

                    mySQLCommand.Parameters.AddWithValue("@CompanyId", companyId);
                    mySQLCommand.Parameters.AddWithValue("@Role", role.RoleName);
                    mySQLCommand.Parameters.AddWithValue("@Description", role.RoleDescription);
                    mySQLCommand.Parameters.AddWithValue("@LastModifyGrants", DateTime.Now);
                    mySQLCommand.Parameters.AddWithValue("@ReadOnly", role.ReadOnly);
                    mySQLCommand.ExecuteNonQuery();

                    mySQLCommand.Dispose();
                }

            }
            else
            {
                if (roleid == -1)
                {
                    //Inserisco i Ruoli
                    string sInsert = @"INSERT INTO MSD_CompanyRoles 
						(CompanyId, Role, Description, LastModifyGrants, ReadOnly)
					  VALUES
						(@CompanyId, @Role, @Description, @LastModifyGrants, @ReadOnly)";

                    SqlCommand mySQLCommand = new SqlCommand(sInsert, sqlOSLConnection);

                    mySQLCommand.Parameters.AddWithValue("@CompanyId", companyId);
                    mySQLCommand.Parameters.AddWithValue("@Role", role.RoleName);
                    mySQLCommand.Parameters.AddWithValue("@Description", role.RoleDescription);
                    mySQLCommand.Parameters.AddWithValue("@LastModifyGrants", DateTime.Now);
                    mySQLCommand.Parameters.AddWithValue("@ReadOnly", role.ReadOnly);
                    mySQLCommand.ExecuteNonQuery();

                    mySQLCommand.Dispose();
                }

            }

			foreach(Grant grantsRow in role.Grats)
			{
				if (grantsRow.NameSpace == string.Empty || grantsRow.TypeId == 0 )
					continue;

				//Controllo se l'oggetto è nel DB
                if (!ExistObject(grantsRow.NameSpace, CommonObjectTreeFunction.GetObjectTypeId(grantsRow.TypeId, sqlOSLConnection), sqlOSLConnection))
					CommonObjectTreeFunction.InsertObjectInDB(new NameSpace(grantsRow.NameSpace), grantsRow.TypeId, sqlOSLConnection);

                int objectId = CommonObjectTreeFunction.GetObjectId(grantsRow.NameSpace, CommonObjectTreeFunction.GetObjectTypeId(grantsRow.TypeId, sqlOSLConnection), sqlOSLConnection);
				if (objectId == -1)
					continue;

				if(!IsProtected(grantsRow.NameSpace, grantsRow.TypeId, companyId, sqlOSLConnection))
					CommonObjectTreeFunction.ProtectObject(companyId, objectId, sqlOSLConnection, true);

                InsertGrants(grantsRow, sqlOSLConnection, companyId, objectId, role.RoleName, connectionString);
			}
			
			return true;
		}

		#endregion   

        //---------------------------------------------------------------------
        private static int GetIdRole(string companyId, string roleName,string sqlOSLConnection)
        {
            SqlDataReader mylocalDataReader = null;
            string myQuery = @"SELECT MSD_CompanyRoles.RoleId FROM MSD_CompanyRoles
								WHERE MSD_CompanyRoles.CompanyId = @CompanyId AND MSD_CompanyRoles.Role = @RoleName";

            int roleId = -1;
            SqlConnection conn = new SqlConnection(sqlOSLConnection);
            conn.Open();
            try
            {
                SqlCommand myCommand = new SqlCommand(myQuery, conn);
                myCommand.Parameters.Add(new SqlParameter("@CompanyId", Int32.Parse(companyId)));
                myCommand.Parameters.Add(new SqlParameter("@RoleName", roleName));
                mylocalDataReader = myCommand.ExecuteReader();
                while (mylocalDataReader.Read())
                    roleId = Convert.ToInt32(mylocalDataReader["RoleId"].ToString());
            }
            catch (SqlException )
            {
            }
            finally
            {
                if (mylocalDataReader != null)
                    mylocalDataReader.Close();
            }

            return roleId;
        }

		//---------------------------------------------------------------------
		static public bool ExistObject(string nameSpace, int codTypeObject, SqlConnection conn)
		{
			if (nameSpace == null || nameSpace == String.Empty || conn == null)
				return false;

			if(conn.State != ConnectionState.Open )
				conn.Open();

			SqlCommand sqlCommand = null;
			SqlDataReader myReader = null;

			try
			{
				string sSelect =@"SELECT ObjectId FROM MSD_Objects WHERE NameSpace = @NameSpace and TypeId = @TypeId";
				
				sqlCommand = new SqlCommand(sSelect, conn);
                sqlCommand.Parameters.AddWithValue("@NameSpace", nameSpace);
                sqlCommand.Parameters.AddWithValue("@TypeId", codTypeObject);

				myReader = sqlCommand.ExecuteReader();
				if (myReader != null && myReader.Read())
				{
					objectId = Convert.ToInt32(myReader["ObjectId"]);
					return true;
				}
			}
			catch(SqlException)
			{
			}
			finally
			{
				if (myReader != null && !myReader.IsClosed) 
					myReader.Close();
				if (sqlCommand != null)
					sqlCommand.Dispose();
			}
			return false;
		}

		//---------------------------------------------------------------------
		public static bool IsProtected(string nameSpace, int codObjectTypeDataBase, int aCompanyId,  SqlConnection conn)
		{
			if (nameSpace == null || nameSpace == String.Empty || conn == null || conn.State != ConnectionState.Open)
				return false;

			SqlCommand mySqlCommandSel = null;

			try
			{
                string sSelect = string.Empty;
                mySqlCommandSel = new SqlCommand();

                if (aCompanyId == 0)
                {
                    sSelect = @"SELECT COUNT(*) FROM MSD_ProtectedObjects 
							INNER JOIN
							MSD_Objects ON 
							MSD_ProtectedObjects.ObjectId = MSD_Objects.ObjectId 
							WHERE NameSpace= @NameSpace AND
							MSD_Objects.TypeId = @TypeId ";
                }
                else
                {
                    sSelect = @"SELECT COUNT(*) FROM MSD_ProtectedObjects 
							INNER JOIN
							MSD_Objects ON 
							MSD_ProtectedObjects.ObjectId = MSD_Objects.ObjectId 
							WHERE NameSpace= @NameSpace AND
							MSD_Objects.TypeId = @TypeId AND
							MSD_ProtectedObjects.CompanyId = @CompanyId";

                    mySqlCommandSel.Parameters.AddWithValue("@CompanyId", aCompanyId);
                }

                mySqlCommandSel.CommandText = sSelect;
                mySqlCommandSel.Connection = conn;



                mySqlCommandSel.Parameters.AddWithValue("@NameSpace", nameSpace);
                mySqlCommandSel.Parameters.AddWithValue("@TypeId", codObjectTypeDataBase);
				return ((int)mySqlCommandSel.ExecuteScalar() > 0);
			}
			catch (SqlException e)
			{
				Debug.Fail("Exception thrown in CommonObjectTreeFunction.IsProtected: " + e.Message);
			}
			finally
			{
				if (mySqlCommandSel != null)
					mySqlCommandSel.Dispose();
			}
			return false;
		}

		//---------------------------------------------------------------------
		public static bool IsProtected(string nameSpace, int codObjectTypeDataBase, SqlConnection conn)
		{
			if (nameSpace == null || nameSpace == String.Empty || conn == null || conn.State != ConnectionState.Open)
				return false;

            return IsProtected(nameSpace, codObjectTypeDataBase, 0, conn);
		}
        //---------------------------------------------------------------------
        static public bool DeleteAllGrantsForRole(int companyId, int LoginOrRoleId, string connection)
        {

            SqlConnection conn = new SqlConnection(connection);
            conn.Open();

            SqlCommand deleteGrantsSqlCommand = null;

            try
            {

                string deleteGrantsQuery = @"DELETE FROM MSD_ObjectGrants 
								WHERE CompanyId = @CompanyId  AND RoleId=@ParamId";

                deleteGrantsSqlCommand = new SqlCommand(deleteGrantsQuery, conn);

                deleteGrantsSqlCommand.Parameters.AddWithValue("@CompanyId", companyId);
                deleteGrantsSqlCommand.Parameters.AddWithValue("@ParamId", LoginOrRoleId);

                deleteGrantsSqlCommand.ExecuteNonQuery();

                return true;
            }
            catch (SqlException err)
            {
                DiagnosticViewer.ShowError(Strings.Error, err.Message, err.Procedure, err.Number.ToString(), SecurityConstString.SecurityAdminPlugIn);

                return false;
            }
            finally
            {
                if (deleteGrantsSqlCommand != null)
                    deleteGrantsSqlCommand.Dispose();
            }
        }
        //---------------------------------------------------------------------
        static public int GetRoleId(string aRole, int aCompanyId, SqlConnection aSqlOSLConnection)
		{
			if (aSqlOSLConnection == null || aSqlOSLConnection.State != ConnectionState.Open || aCompanyId == -1 || aRole == null || aRole == String.Empty)
				return -1;
			
			SqlCommand selectRoleCommand = null;
			SqlDataReader myReader = null;

			try
			{
				//Cerco l'id del Ruolo che hoinserito nel DB ma che ho sul file 
				//come descrizione
				string sSelect = @"SELECT RoleId FROM MSD_CompanyRoles
								WHERE CompanyId = @CompanyId AND Role = @Role";
			
				selectRoleCommand = new SqlCommand(sSelect, aSqlOSLConnection);
                selectRoleCommand.Parameters.AddWithValue("@CompanyId", aCompanyId);
                selectRoleCommand.Parameters.AddWithValue("@Role", aRole);

				myReader = selectRoleCommand.ExecuteReader();

				if(myReader!= null && myReader.Read())
					return Convert.ToInt32(myReader["RoleId"]);
			}	
			catch (SqlException e)
			{
				Debug.Fail("Exception thrown in CommonObjectTreeFunction.GetRoleId: " + e.Message);
			}
			finally
			{
				if (myReader != null && !myReader.IsClosed) 
					myReader.Close();
				if (selectRoleCommand != null)
					selectRoleCommand.Dispose();
			}
			return -1;
		}
		
	
		//---------------------------------------------------------------------
		private static bool ExistObjectsInDB(SqlConnection sqlOSLConnection)
		{
			if (sqlOSLConnection == null || sqlOSLConnection.State != ConnectionState.Open)
				return false;
			
			SqlCommand sqlCommand = null;

			try
			{
				string sSelect = @"SELECT COUNT(*) FROM MSD_Objects";
				sqlCommand = new SqlCommand(sSelect, sqlOSLConnection);

				int numRow = (int)sqlCommand.ExecuteScalar();
				
				sqlCommand.Dispose();

				return (numRow > 0);
			}
			catch(SqlException)
			{
				if (sqlCommand != null)
					sqlCommand.Dispose();
				
				return false;
			}
		}

		//---------------------------------------------------------------------

	}
	//=========================================================================
	/// <summary>
	/// Classe contenente funzioni comunia più parti del codice che lavorano sui
	/// nodi del MenuManager
	/// </summary>
	public class CommonObjectTreeFunction
	{
		static private SqlCommand		selectObjectIdSqlCommand		= null;
		static private SqlCommand		insertObjectIdSqlCommand		= null;
		static private	string selectObjectChildrenQueryTableName	= "MSD_Objects";
		static private string objNamespaceColumnName				= "NameSpace";
		static private  string objTypeIdColumnName				= "TypeId";
		static private string objParentIdColumnName				= "ParentId";

		//---------------------------------------------------------------------
		public static  bool CreateSelectObjectIdSqlCommand(SqlConnection sqlOSLConnection)
		{
			if (selectObjectIdSqlCommand != null) 
				return true;

			if (sqlOSLConnection != null && sqlOSLConnection.State == ConnectionState.Open)
			{
				string selectObjectIdQuery = @"select ObjectId from @" + selectObjectChildrenQueryTableName + " where NameSpace = @" + objNamespaceColumnName;
				
				selectObjectIdSqlCommand = new SqlCommand(selectObjectIdQuery, sqlOSLConnection);
				selectObjectIdSqlCommand.Parameters.Add("@" + selectObjectChildrenQueryTableName,  SqlDbType.NVarChar, 512, selectObjectChildrenQueryTableName);
				selectObjectIdSqlCommand.Parameters.Add("@" + objNamespaceColumnName,  SqlDbType.NVarChar, 512, objNamespaceColumnName);
				return true;
			}
			return false;
		}


		//---------------------------------------------------------------------
		public static  bool CreateInsertObjectIdSqlCommand(SqlConnection sqlOSLConnection)
		{
			try
			{
				if (insertObjectIdSqlCommand != null) 
					return true;

				string insertObjectQuery =
					@"INSERT INTO @" + selectObjectChildrenQueryTableName + 
					@" (TypeId, NameSpace, ParentId) VALUES (@" 
					+ objTypeIdColumnName + ", @" + objNamespaceColumnName + ", @" + objParentIdColumnName + ")";
				
				insertObjectIdSqlCommand =  new SqlCommand(insertObjectQuery,  sqlOSLConnection);

                insertObjectIdSqlCommand.Parameters.AddWithValue("@" + selectObjectChildrenQueryTableName, selectObjectChildrenQueryTableName);
                insertObjectIdSqlCommand.Parameters.AddWithValue("@" + objTypeIdColumnName, objTypeIdColumnName);
                insertObjectIdSqlCommand.Parameters.AddWithValue("@" + objNamespaceColumnName, objNamespaceColumnName);
                insertObjectIdSqlCommand.Parameters.AddWithValue("@" + objParentIdColumnName, objParentIdColumnName);
				return true;
			}
			catch (SqlException )
			{
				insertObjectIdSqlCommand = null;
				return false;
			}
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che inserisce nella MSD_Objects gli oggetti estratti dal DOM
		/// </summary>
		/// <param name="currNode"></param>
		/// <param name="parentNode"></param>
		public static void WriteToDB(MenuXmlNode currNode, SqlConnection sqlOSLConnection)
		{
			if (sqlOSLConnection == null || sqlOSLConnection.State != ConnectionState.Open)
				return;

			if (currNode == null || currNode.ItemObject == null || currNode.ItemObject == String.Empty ) 
				return;

			if (CommonObjectTreeFunction.GetObjectId(currNode, sqlOSLConnection)!= -1)
				return;

			if (selectObjectIdSqlCommand == null)//||  insertObjectIdSqlCommand.Connection != sqlOSLConnection)
			{
				string selectObjectIdQuery = @"SELECT ObjectId FROM MSD_Objects where NameSpace = @NameSpace AND TypeID = @TypeId";
				
				selectObjectIdSqlCommand = new SqlCommand(selectObjectIdQuery, sqlOSLConnection);

				selectObjectIdSqlCommand.Parameters.Add("@NameSpace",  SqlDbType.NVarChar, 512, "NameSpace");
				selectObjectIdSqlCommand.Parameters.Add("@TypeId",  SqlDbType.Int, 4 , "TypeId");
			}

            if (insertObjectIdSqlCommand == null || insertObjectIdSqlCommand.Connection != sqlOSLConnection)
            {
                if (insertObjectIdSqlCommand != null)
                    insertObjectIdSqlCommand.Dispose();

                string insertObjectQuery = @"INSERT INTO MSD_Objects" +
                    @"(TypeId, NameSpace, ParentId) VALUES
											(@TypeId, @NameSpace, @ParentId)";

                insertObjectIdSqlCommand = new SqlCommand(insertObjectQuery, sqlOSLConnection);
                insertObjectIdSqlCommand.Parameters.Add("@TypeId", SqlDbType.Int, 4, "TypeId");
                insertObjectIdSqlCommand.Parameters.Add("@NameSpace", SqlDbType.NVarChar, 512, "NameSpace");
                insertObjectIdSqlCommand.Parameters.Add("@ParentId", SqlDbType.Int, 4, "ParentId");
            }

            SqlDataReader myReader = null;
			try
			{
				int		currentParentId = 0;
				int		parentTypeId	= 0;
				string	parentNameSpace = "";

				MenuXmlNode parentNode = currNode.GetParentNode();
				
				if (parentNode != null && parentNode.ItemObject!= null && !currNode.IsRunReport && 
					!(currNode.IsExternalItem && (currNode.ExternalItemType == SecurityType.Report.ToString()))) 
				{
					//Allora ho un padre quindi lo cerco nel DB x mettergli l'id giusto
					parentNameSpace = parentNode.ItemObject;

                    parentTypeId = CommonObjectTreeFunction.GetObjectTypeId(parentNode, sqlOSLConnection); 

					if (parentTypeId != -1)
					{
                        selectObjectIdSqlCommand.Parameters["@NameSpace"].Value = parentNameSpace;
						selectObjectIdSqlCommand.Parameters["@TypeId"].Value	= parentTypeId;
					
						myReader = selectObjectIdSqlCommand.ExecuteReader();
				
						if (myReader.Read())
							currentParentId = Convert.ToInt32(myReader["objectid"]);
					
						myReader.Close();
						myReader = null;
					}
				}



                int typeid = CommonObjectTreeFunction.GetObjectTypeId(currNode, sqlOSLConnection);
                if (currNode.ItemObject.Contains(NameSolverStrings.EasyStudio) && currNode.IsRunDocument)
                    typeid = GetObjectTypeId((int)SecurityType.Function, sqlOSLConnection);
                insertObjectIdSqlCommand.Parameters["@TypeId"].Value = typeid;
				insertObjectIdSqlCommand.Parameters["@NameSpace"].Value = currNode.ItemObject;
				insertObjectIdSqlCommand.Parameters["@ParentId"].Value  = currentParentId;
				
				insertObjectIdSqlCommand.ExecuteNonQuery();
			}
			catch (SqlException exx)
			{
                string a = exx.Message;

				if (myReader != null && !myReader.IsClosed)
					myReader.Close();
				if (selectObjectIdSqlCommand != null)
					selectObjectIdSqlCommand.Dispose();
				
				selectObjectIdSqlCommand = null;

				if (insertObjectIdSqlCommand != null)
					insertObjectIdSqlCommand.Dispose();
				
				insertObjectIdSqlCommand = null;
			}
		}
		#region GetCompanyName
		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che ritorna il nome dell'Azienda passandogli il suo id
		/// </summary>
		/// <param name="aCompanyId"></param>
		/// <param name="sqlOSLConnection"></param>
		/// <returns></returns>
		public static string GetCompanyName(int aCompanyId, SqlConnection sqlOSLConnection)
		{
			if (aCompanyId == -1 || sqlOSLConnection == null || sqlOSLConnection.State != ConnectionState.Open)
				return String.Empty;

			string select = "SELECT Company FROM MSD_Companies WHERE CompanyId = " + aCompanyId.ToString();

			SqlCommand myCommand  = null;
			SqlDataReader myReader = null;
			string company = String.Empty;
			try
			{
				myCommand  = new SqlCommand(select, sqlOSLConnection);
				myReader = myCommand.ExecuteReader();
				if (myReader.Read())
					company = myReader["Company"].ToString();
				myReader.Close();
			}
			catch (Exception )
			{
				if (myReader != null && !myReader.IsClosed)
					myReader.Close();
				if (myCommand != null)
					myCommand.Dispose();
			}
			return company;
		}

		#endregion

		#region funzioni di GET sull'oggetto TypeId
		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che restituisce il tipo di oggetto che ho selezionato
		/// </summary>
		/// <param name="nodeType"></param>
		/// <returns></returns>
		public static int GetObjectTypeId(MenuXmlNode node, SqlConnection sqlOSLConnection)
		{
            int codeType;
            return GetObjectTypeId(node, sqlOSLConnection, out codeType);
		}

        /// <summary>
        /// Funzione che restituisce il tipo di oggetto che ho selezionato
        /// </summary>
        /// <param name="nodeType"></param>
        /// <returns></returns>
        public static int GetObjectTypeId(MenuXmlNode node, SqlConnection sqlOSLConnection, out int codeType)
        {
            codeType = 0;
            if (node == null || sqlOSLConnection == null || sqlOSLConnection.State != ConnectionState.Open)
                return -1;

            codeType = MenuSecurityFilter.GetType(node);

            return GetObjectTypeId(codeType, sqlOSLConnection);
        }
	
		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che restituisce il tipo di oggetto che ho selezionato
		/// </summary>
		/// <param name="nodeType"></param>
		/// <returns></returns>
		public static int GetObjectTypeId(int type, SqlConnection sqlOSLConnection)
		{
			if (sqlOSLConnection == null || sqlOSLConnection.State != ConnectionState.Open)
				return -1;

						
			SqlCommand mySqlCommandSel = null;
			SqlDataReader myReader = null;
			try
			{
				string sSelect = @"SELECT TypeId FROM MSD_ObjectTypes WHERE Type= @Type";
				mySqlCommandSel = new SqlCommand(sSelect, sqlOSLConnection);
                mySqlCommandSel.Parameters.AddWithValue("@Type", type);

				myReader = mySqlCommandSel.ExecuteReader();
				if (myReader != null && myReader.Read())
					return Convert.ToInt32(myReader["TypeId"]);
			}
			catch (SqlException err)
			{
				DiagnosticViewer.ShowError(Strings.Error, err.Message, err.Procedure, err.Number.ToString(), SecurityConstString.SecurityAdminPlugIn); 
			}
			finally
			{
				if (myReader != null && !myReader.IsClosed)
					myReader.Close();
				if (mySqlCommandSel != null)
					mySqlCommandSel.Dispose();
			}
			return -1;
		}

		//---------------------------------------------------------------------
		static public int GetObjectId(string objectNameSpace, int objectType, string sqlOSLConnectionString)
		{
			SqlConnection conn = new SqlConnection(sqlOSLConnectionString);
			conn.Open();
			return GetObjectId(objectNameSpace, objectType, conn);
		}
		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che ritorna l'ObjectId passando il nodo rappresentante l'oggetto 
		/// stesso
		/// </summary>
		/// <param name="aMenuXmlNode"></param>
		/// <param name="sqlOSLConnection"></param>
		/// <returns></returns>
		static public int GetObjectId(MenuXmlNode aMenuXmlNode, SqlConnection sqlOSLConnection)
		{
			if 
				(
				aMenuXmlNode == null || 
				aMenuXmlNode.ItemObject == null || 
				aMenuXmlNode.ItemObject == String.Empty || 
				sqlOSLConnection == null || 
				sqlOSLConnection.State != ConnectionState.Open
				)
				return -1;

            return GetObjectId(aMenuXmlNode.ItemObject, GetObjectTypeId(aMenuXmlNode, sqlOSLConnection), sqlOSLConnection);
		}
		
		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che ritorna l'objectId passandogli il NameSpace e il tipo 
		/// dell'oggetto
		/// </summary>
		/// <param name="objectNameSpace"></param>
		/// <param name="objectType"></param>
		/// <param name="sqlOSLConnection"></param>
		/// <returns></returns>
		static public int GetObjectId(string objectNameSpace, int objectType, SqlConnection sqlOSLConnection)
		{
			if 
				(
					objectNameSpace == null || 
					objectNameSpace == String.Empty || 
					sqlOSLConnection == null || 
					sqlOSLConnection.State != ConnectionState.Open
				)
				return -1;

			SqlCommand		mySqlCommandSel = null;
			SqlDataReader	myReader = null;
			try
			{
				string sSelect = @"SELECT ObjectId FROM MSD_Objects
							WHERE NameSpace = @NameSpace AND TypeId= @TypeId ";

				mySqlCommandSel = new SqlCommand(sSelect, sqlOSLConnection);
                mySqlCommandSel.Parameters.AddWithValue("@NameSpace", objectNameSpace);
                mySqlCommandSel.Parameters.AddWithValue("@TypeId", objectType);
				
				myReader = mySqlCommandSel.ExecuteReader();
				
				if (myReader != null && myReader.Read())
					return Convert.ToInt32 (myReader["ObjectId"]);
				
			}
			catch (SqlException e)
			{
				Debug.Fail("Exception thrown in CommonObjectTreeFunction.GetObjectId: " + e.Message);
			}
			finally
			{
				if (myReader != null && !myReader.IsClosed)
					myReader.Close();
				if (mySqlCommandSel != null)
					mySqlCommandSel.Dispose();
			}
			return -1;
		}

		#endregion

		#region  GetSecurityNodeType
		
		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che ritorna l'enumerativo di tipo SecurityType corrispondente 
		/// al nodo passatogli come parametro
		/// </summary>
		/// <param name="aCommandNode"></param>
		/// <returns></returns>
		static public SecurityType GetSecurityNodeTypeFromCommandNode(MenuXmlNode aCommandNode)
		{
			if (aCommandNode == null || !aCommandNode.IsCommand)
				return SecurityType.Undefined;

			if (aCommandNode.IsRunDocument)
				return SecurityType.DataEntry;

			if (aCommandNode.IsRunReport)
				return SecurityType.Report;

			if (aCommandNode.IsRunBatch)
				return SecurityType.Batch;

			if (aCommandNode.IsRunFunction)
				return SecurityType.Function;

			if (aCommandNode.IsExternalItem)
				return GetSecurityNodeTypeValueFromString(aCommandNode.ExternalItemType);

            if (aCommandNode.IsWordDocument || aCommandNode.IsWordDocument2007)
				return SecurityType.WordDocument;

            if (aCommandNode.IsExcelDocument || aCommandNode.IsExcelDocument2007)
				return SecurityType.ExcelDocument;

            if (aCommandNode.IsWordTemplate || aCommandNode.IsWordTemplate2007)
				return SecurityType.WordTemplate;

            if (aCommandNode.IsExcelTemplate || aCommandNode.IsExcelTemplate2007)
				return SecurityType.ExcelTemplate;

			if (aCommandNode.IsExeShortcut)
				return SecurityType.ExeShortcut;

			if (aCommandNode.IsRunExecutable)
				return SecurityType.Executable;

			return SecurityType.Undefined;
		}
		
		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che ritorna l'enumerativo di tipo SecurityType corrispondente 
		/// alla stringa passatagli come parametro
		/// </summary>
		/// <param name="aTypeString"></param>
		/// <returns></returns>
		static public SecurityType GetSecurityNodeTypeValueFromString(string aTypeString)
		{
			if (aTypeString == null || aTypeString == String.Empty)
				return SecurityType.Undefined;

			if (String.Compare(aTypeString, SecurityType.Function.ToString(),true, CultureInfo.InvariantCulture) == 0)
				return SecurityType.Function;

			if (String.Compare(aTypeString, SecurityType.Report.ToString(), true, CultureInfo.InvariantCulture) == 0)
				return SecurityType.Report;

			if (String.Compare(aTypeString, SecurityType.DataEntry.ToString(), true, CultureInfo.InvariantCulture) == 0)
				return SecurityType.DataEntry;

			if (String.Compare(aTypeString, SecurityType.ChildForm.ToString(), true, CultureInfo.InvariantCulture) == 0)
				return SecurityType.ChildForm;

			if (String.Compare(aTypeString, SecurityType.Batch.ToString(), true, CultureInfo.InvariantCulture) == 0)
				return SecurityType.Batch;

			if (String.Compare(aTypeString, SecurityType.Tab.ToString(), true, CultureInfo.InvariantCulture) == 0)
				return SecurityType.Tab;

            //if (String.Compare(aTypeString, SecurityType.Constraint.ToString(), true, CultureInfo.InvariantCulture) == 0)
            //    return SecurityType.Constraint;
			
			if (String.Compare(aTypeString, SecurityType.Table.ToString(), true, CultureInfo.InvariantCulture) == 0)
				return SecurityType.Table;
			
			if (String.Compare(aTypeString, SecurityType.HotKeyLink.ToString(), true, CultureInfo.InvariantCulture) == 0)
				return SecurityType.HotLink;
			if (String.Compare(aTypeString, SecurityType.HotLink.ToString(), true, CultureInfo.InvariantCulture) == 0)
				return SecurityType.HotLink;
		
			if (String.Compare(aTypeString, SecurityType.View.ToString(), true, CultureInfo.InvariantCulture) == 0)
				return SecurityType.View;
			
			if (String.Compare(aTypeString, SecurityType.RowView.ToString(), true, CultureInfo.InvariantCulture) == 0)
				return SecurityType.RowView;
			
			if (String.Compare(aTypeString, SecurityType.Grid.ToString(), true, CultureInfo.InvariantCulture) == 0)
				return SecurityType.Grid;
			
			if (String.Compare(aTypeString, SecurityType.GridColumn.ToString(), true, CultureInfo.InvariantCulture) == 0)
				return SecurityType.GridColumn;
			
			if (String.Compare(aTypeString, SecurityType.Control.ToString(), true, CultureInfo.InvariantCulture) == 0)
				return SecurityType.Control;
			
			if (String.Compare(aTypeString, SecurityType.Finder.ToString(), true, CultureInfo.InvariantCulture) == 0)
				return SecurityType.Finder;
			
			if (String.Compare(aTypeString, SecurityType.WordDocument.ToString(), true, CultureInfo.InvariantCulture) == 0)
				return SecurityType.WordDocument;

			if (String.Compare(aTypeString, SecurityType.ExcelDocument.ToString(), true, CultureInfo.InvariantCulture) == 0)
				return SecurityType.ExcelDocument;

			if (String.Compare(aTypeString, SecurityType.WordTemplate.ToString(), true, CultureInfo.InvariantCulture) == 0)
				return SecurityType.WordTemplate;

			if (String.Compare(aTypeString, SecurityType.ExcelTemplate.ToString(), true, CultureInfo.InvariantCulture) == 0)
				return SecurityType.ExcelTemplate;

            if (String.Compare(aTypeString, SecurityType.Tabber.ToString(), true, CultureInfo.InvariantCulture) == 0)
                return SecurityType.Tabber;

            if (String.Compare(aTypeString, SecurityType.TileManager.ToString(), true, CultureInfo.InvariantCulture) == 0)
                return SecurityType.TileManager;
            if (String.Compare(aTypeString, SecurityType.Tile.ToString(), true, CultureInfo.InvariantCulture) == 0)
                return SecurityType.Tile;

            if (String.Compare(aTypeString, SecurityType.Toolbar.ToString(), true, CultureInfo.InvariantCulture) == 0)
                return SecurityType.Toolbar;
            if (String.Compare(aTypeString, SecurityType.ToolbarButton.ToString(), true, CultureInfo.InvariantCulture) == 0)
                return SecurityType.ToolbarButton;

            if (String.Compare(aTypeString, SecurityType.EmbeddedView.ToString(), true, CultureInfo.InvariantCulture) == 0)
                return SecurityType.EmbeddedView;

            if (String.Compare(aTypeString, SecurityType.PropertyGrid.ToString(), true, CultureInfo.InvariantCulture) == 0)
                return SecurityType.PropertyGrid;

            if (String.Compare(aTypeString, SecurityType.TilePanelTab.ToString(), true, CultureInfo.InvariantCulture) == 0)
                return SecurityType.TilePanelTab;

			return SecurityType.Undefined;
		}
		
		#endregion

		#region funzioni sulla protezione /sprotezione degli oggetti

		#region la chiamo x proteggere e sproteggere
		/// <summary>
		/// Funzione che in base al booleano protect richiama le funzioni di protezione
		/// o sprotezione sul nodo passatogli come parametro
		/// </summary>
		/// <param name="aMenuXmlNode"></param>
		/// <param name="protect"></param>
		/// <param name="companyId"></param>
		/// <param name="sqlOSLConnection"></param>
		/// <returns></returns>
		static public bool SetObjectProtection(MenuXmlNode aMenuXmlNode, bool protect, int companyId, SqlConnection sqlOSLConnection)
		{
			if (protect)
				return ProtectObject(aMenuXmlNode, companyId, sqlOSLConnection);

			return UnprotectObject(aMenuXmlNode, companyId, sqlOSLConnection);
		}

		#endregion

		#region funzioni che inseriscono il dato nella MSD_ProtectedObjects
		
		/// <summary>
		/// Funzione che inserisce l'oggetto sotto protezione
		/// </summary>
		/// <param name="node"></param>
		/// <param name="companyId"></param>
		/// <param name="sqlOSLConnection"></param>
		/// <returns></returns>
		static private bool ProtectObject(MenuXmlNode node, int companyId, SqlConnection sqlOSLConnection)
		{
			if (node == null || node.ItemObject == null || node.ItemObject == String.Empty) 
				return false;

			int objectId = GetObjectId(node, sqlOSLConnection);
			if (objectId == -1)
			{
				if(InsertObjectInDB(node, sqlOSLConnection))
					objectId = GetObjectId(node, sqlOSLConnection);
				else
					return false;
			}

            if (!ImportExportFunction.IsProtected(node.ItemObject, GetObjectTypeId(node, sqlOSLConnection), companyId, sqlOSLConnection))
            {
                if (!ProtectObject(companyId, objectId, sqlOSLConnection, false))
                    return false;
            }

			node.ProtectedState	= true;
			return true;
		}


        static public bool ProtectObject(int companyId, int objectId, SqlConnection sqlOSLConnection, bool isFromGrant)
        {
            if
                (
                objectId == -1 ||
                sqlOSLConnection == null ||
                sqlOSLConnection.State != ConnectionState.Open
                )
                return false;

            SqlCommand mySqlCommandInsert = null;
            try
            {
                //Inserisco riga nella MSD_ProtectedObjects
                string sInsert = @"INSERT INTO MSD_ProtectedObjects
							(CompanyId, ObjectId, Disabled, FromGrant)
							VALUES 
							(@CompanyId, @ObjectId, 0, @FromGrant)";

                mySqlCommandInsert = new SqlCommand(sInsert, sqlOSLConnection);

                mySqlCommandInsert.Parameters.AddWithValue("@CompanyId", companyId);
                mySqlCommandInsert.Parameters.AddWithValue("@ObjectId", objectId);
                mySqlCommandInsert.Parameters.AddWithValue("@FromGrant", isFromGrant);

                mySqlCommandInsert.ExecuteNonQuery();

                return true;
            }
            catch (SqlException)
            {
                //			Debug.Fail("Exception thrown in CommonObjectTreeFunction.ProtectObject: " + e.Message);

                return false;
            }
            finally
            {
                if (mySqlCommandInsert != null)
                    mySqlCommandInsert.Dispose();
            }
        }

        #endregion

        #region funzioni che cancellano il dato dalla MSD_ProtectedObjects

        /// <summary>
        /// Funzione  che elimina la protezione sull'oggetto
        /// </summary>
        /// <param name="node"></param>
        /// <param name="companyId"></param>
        /// <param name="sqlOSLConnection"></param>
        /// <returns></returns>
        static private bool UnprotectObject(MenuXmlNode node, int companyId, SqlConnection sqlOSLConnection)
		{
			if (node == null || node.ItemObject == null || node.ItemObject == String.Empty) 
				return false;

			if (!UnprotectObject(companyId, GetObjectId(node, sqlOSLConnection), sqlOSLConnection))
				return false;

			node.ProtectedState	= false;
			return true;
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che prima cancella i grant assegnati all'oggetto selezionato 
		/// nella tabella MSD_ObjectGrants e poi l'oggetto stesso dalla tabella 
		/// MSD_ProtectedObjects
		/// </summary>
		static private bool UnprotectObject(int companyId, int objectId, SqlConnection sqlOSLConnection)
		{
			if 
				(
				objectId == -1 || 
				sqlOSLConnection == null || 
				sqlOSLConnection.State != ConnectionState.Open
				)
				return false;

			SqlCommand deleteObjectGrantsSqlCommand = null;
			SqlCommand deleteObjectProtectionSqlCommand = null;

			try
			{
				//Cancello gli eventuali Grants
				string deleteGrantsQuery= @"DELETE FROM MSD_ObjectGrants WHERE ObjectId = @ObjectId	AND CompanyId =@CompanyId";
				
				deleteObjectGrantsSqlCommand = new SqlCommand(deleteGrantsQuery, sqlOSLConnection);

                deleteObjectGrantsSqlCommand.Parameters.AddWithValue("@ObjectId", objectId);
                deleteObjectGrantsSqlCommand.Parameters.AddWithValue("@CompanyId", companyId);

				deleteObjectGrantsSqlCommand.ExecuteNonQuery();
				
				//Cancello l'eventuale riga nella MSD_ProtectedObjects
				string deleteProtectionQuery = @"DELETE FROM MSD_ProtectedObjects WHERE ObjectId = @ObjectId AND CompanyId =@CompanyId";

				deleteObjectProtectionSqlCommand = new SqlCommand(deleteProtectionQuery, sqlOSLConnection);

                deleteObjectProtectionSqlCommand.Parameters.AddWithValue("@ObjectId", objectId);
                deleteObjectProtectionSqlCommand.Parameters.AddWithValue("@CompanyId", companyId);
				
				deleteObjectProtectionSqlCommand.ExecuteNonQuery();

				return true;
			}
			catch (SqlException e)
			{
				DiagnosticViewer.ShowError(Strings.Error, e.Message, e.Procedure, e.Number.ToString(), SecurityConstString.SecurityAdminPlugIn);

				return false;
			}
			finally
			{
				if (deleteObjectGrantsSqlCommand != null)
					deleteObjectGrantsSqlCommand.Dispose();

				if (deleteObjectProtectionSqlCommand != null)
					deleteObjectProtectionSqlCommand.Dispose();
			}
		}

		#endregion

		#endregion

		#region Cancella permessi
		
		/// <summary>
		/// Funzione che cancella i permessi sull'oggetto
		/// </summary>
		/// <param name="node"></param>
		/// <param name="companyId"></param>
		/// <param name="isRoleGrants"></param>
		/// <param name="LoginOrRoleId"></param>
		/// <param name="sqlOSLConnection"></param>
		/// <returns></returns>
		static public bool DeleteGrants(MenuXmlNode node, int companyId, bool isRoleGrants, int LoginOrRoleId, SqlConnection sqlOSLConnection)
		{
			if (node == null || node.ItemObject == null || node.ItemObject == String.Empty) 
				return false;	
			
			return DeleteGrants(companyId, GetObjectId(node, sqlOSLConnection), isRoleGrants, LoginOrRoleId, sqlOSLConnection);
		}
		
		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che cancella fisicamente i record riguardanti l'oggetto selezionato
		/// dalla tabella MSD_ObjectGrants
		/// </summary>
		/// <param name="companyId"></param>
		/// <param name="objectId"></param>
		/// <param name="isRoleGrants"></param>
		/// <param name="LoginOrRoleId"></param>
		/// <param name="sqlOSLConnection"></param>
		/// <returns></returns>
		static public bool DeleteGrants(int companyId, int objectId, bool isRoleGrants, int LoginOrRoleId, SqlConnection sqlOSLConnection)			
			
		{
			if 
				(
					objectId == -1 || 
					sqlOSLConnection == null || 
					sqlOSLConnection.State != ConnectionState.Open
				)
				return false;
			
			SqlCommand deleteGrantsSqlCommand = null;

			try
			{
				string deleteGrantsQuery = String.Empty; 
				if (isRoleGrants) //Cancello i Permessi attribuiti ai ruoli
					deleteGrantsQuery = @"DELETE FROM MSD_ObjectGrants 
								WHERE CompanyId = @CompanyId AND ObjectId=@ObjectId AND RoleId=@ParamId";
				else //Cancello i Permessi attribuiti agli utenti
					deleteGrantsQuery = @"DELETE FROM MSD_ObjectGrants 
								WHERE CompanyId = @CompanyId AND ObjectId=@ObjectId AND LoginId=@ParamId";

				deleteGrantsSqlCommand = new SqlCommand(deleteGrantsQuery, sqlOSLConnection);

                deleteGrantsSqlCommand.Parameters.AddWithValue("@CompanyId", companyId);
                deleteGrantsSqlCommand.Parameters.AddWithValue("@ObjectId", objectId);
                deleteGrantsSqlCommand.Parameters.AddWithValue("@ParamId", LoginOrRoleId);

				deleteGrantsSqlCommand.ExecuteNonQuery();
				
				return true;
			}
			catch (SqlException err)
			{
				DiagnosticViewer.ShowError(Strings.Error, err.Message, err.Procedure, err.Number.ToString(), SecurityConstString.SecurityAdminPlugIn);
				
				return false;
			}
			finally
			{
				if (deleteGrantsSqlCommand != null)
					deleteGrantsSqlCommand.Dispose();	
			}
		}

		#endregion

		#region Operazioni sull'attribuzione grants

		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che carica in una DataTable gli eventuali permessi presenti 
		/// nel DB per l'oggetto selezionato
		/// </summary>
		/// <param name="isUser"></param>
		/// <param name="aMenuXmlNode"></param>
		/// <param name="companyId"></param>
		/// <param name="codSelTree"></param>
		/// <param name="sqlOSLConnection"></param>
		/// <param name="arrayTypeGrants"></param>
		/// <param name="objectType"></param>
		/// <returns></returns>
		public static DataTable LoadOldValues(bool isUser, MenuXmlNode aMenuXmlNode, int companyId, 
											int codSelTree, SqlConnection sqlOSLConnection, ArrayList arrayTypeGrants, int objectType )
		{
			GrantsDataGrid dg = new GrantsDataGrid(!isUser);
			int objectId = GetObjectId(aMenuXmlNode.ItemObject, objectType, sqlOSLConnection);
			LoadStoredProcedure(sqlOSLConnection, ref arrayTypeGrants, !isUser, companyId, codSelTree, objectId);
			for (int i=0; i < arrayTypeGrants.Count; i++)
			{
				if (arrayTypeGrants[i] != null && arrayTypeGrants[i] is GrantsRow)
					dg.AddGrantsRow((GrantsRow)arrayTypeGrants[i]);
			}
			
			return (DataTable)dg.DataSource.Tables["Grants"];
		}
		
		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che aggiunge nuovi valori alla DataTable dei permessi preesistenti
		/// </summary>
		/// <param name="oldDataTable"></param>
		/// <param name="dataTable"></param>
		public static  void AddNewValueInOldDataTable(ref DataTable oldDataTable, ref DataTable dataTable)
		{
			DataRow[] drByGrants = null;
			foreach (DataRow dr in dataTable.Rows)
			{
				drByGrants = oldDataTable.Select( securityGrants.Grant  + " ='"+ dr[ securityGrants.Grant] + "'");
				if (drByGrants == null || drByGrants.Length == 0 ) 
					continue;
				oldDataTable.Select( securityGrants.Grant  + " ='"+ dr[ securityGrants.Grant] + "'")[0][securityGrants.Assign] = dr[5];

			}
		}
		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che esegue la StoreProcedure che legge i permessi su DataBase
		/// </summary>
		/// <param name="sqlConnection"></param>
		/// <param name="grantsArrayList"></param>
		/// <param name="isRole"></param>
		/// <param name="companyId"></param>
		/// <param name="roleOrUserId"></param>
		/// <param name="objectId"></param>
		public static void LoadStoredProcedure(SqlConnection sqlConnection, 
												ref ArrayList grantsArrayList, 
												bool isRole, int companyId, int roleOrUserId, int objectId)
		{
			if (sqlConnection == null || sqlConnection.State != ConnectionState.Open)
				return;

			SqlCommand storedProcedureCommand = null;

			try
			{
				string commandText = isRole ? "MSD_GetSplitRoleGrant" : "MSD_GetSplitUserGrant";
				
				storedProcedureCommand = new SqlCommand(commandText, sqlConnection);

				storedProcedureCommand.CommandType = CommandType.StoredProcedure;

				if (!isRole)
				{
					storedProcedureCommand.Parameters.Add("@parout_thereis_usrgrant", SqlDbType.Int).Value = 1;
					storedProcedureCommand.Parameters.Add("@parout_usr_grant", SqlDbType.Int).Value        = 1;
					storedProcedureCommand.Parameters.Add("@parout_usr_inheritmask", SqlDbType.Int).Value  = 1;

					storedProcedureCommand.Parameters["@parout_thereis_usrgrant"].Direction	= ParameterDirection.Output;
					storedProcedureCommand.Parameters["@parout_usr_grant"].Direction			= ParameterDirection.Output;
					storedProcedureCommand.Parameters["@parout_usr_inheritmask"].Direction	= ParameterDirection.Output;
				}
				storedProcedureCommand.Parameters.Add("@par_companyid", SqlDbType.Int).Value = companyId;
				
				if (isRole)
					storedProcedureCommand.Parameters.Add("@par_roleid", SqlDbType.Int).Value = roleOrUserId;
				else
					storedProcedureCommand.Parameters.Add("@par_userid", SqlDbType.Int).Value = roleOrUserId;
					
				storedProcedureCommand.Parameters.Add("@par_objectid", SqlDbType.Int).Value               = objectId;
				storedProcedureCommand.Parameters.Add("@parout_thereis_rolegrant", SqlDbType.Int).Value   = 1;
				storedProcedureCommand.Parameters.Add("@parout_role_grant", SqlDbType.Int).Value          = 1;
				storedProcedureCommand.Parameters.Add("@parout_role_inheritmask", SqlDbType.Int).Value    = 1;
				storedProcedureCommand.Parameters.Add("@parout_thereis_parentgrant", SqlDbType.Int).Value = 1;
				storedProcedureCommand.Parameters.Add("@parout_parent_grant", SqlDbType.Int).Value        = 1;
				storedProcedureCommand.Parameters.Add("@parout_parent_inheritmask", SqlDbType.Int).Value  = 1;
				storedProcedureCommand.Parameters.Add("@parout_thereis_totalgrant", SqlDbType.Int).Value  = 1;
				storedProcedureCommand.Parameters.Add("@parout_total_grant", SqlDbType.Int).Value         = 1;
				storedProcedureCommand.Parameters.Add("@parout_total_inheritmask", SqlDbType.Int).Value   = 1;
				storedProcedureCommand.Parameters.Add("@parout_protected_object", SqlDbType.Int).Value    = 1;
				storedProcedureCommand.Parameters.Add("@parout_existparent_object", SqlDbType.Int).Value  = 1;
					
				storedProcedureCommand.Parameters["@parout_thereis_rolegrant"].Direction   = ParameterDirection.Output;
				storedProcedureCommand.Parameters["@parout_role_grant"].Direction          = ParameterDirection.Output;
				storedProcedureCommand.Parameters["@parout_role_inheritmask"].Direction    = ParameterDirection.Output;
				storedProcedureCommand.Parameters["@parout_thereis_parentgrant"].Direction = ParameterDirection.Output;
				storedProcedureCommand.Parameters["@parout_parent_grant"].Direction        = ParameterDirection.Output;
				storedProcedureCommand.Parameters["@parout_parent_inheritmask"].Direction  = ParameterDirection.Output;
				storedProcedureCommand.Parameters["@parout_thereis_totalgrant"].Direction  = ParameterDirection.Output;
				storedProcedureCommand.Parameters["@parout_total_grant"].Direction         = ParameterDirection.Output;
				storedProcedureCommand.Parameters["@parout_total_inheritmask"].Direction   = ParameterDirection.Output;
				storedProcedureCommand.Parameters["@parout_protected_object"].Direction    = ParameterDirection.Output;
				storedProcedureCommand.Parameters["@parout_existparent_object"].Direction  = ParameterDirection.Output;

				storedProcedureCommand.Parameters.Add("@ReturnVal", SqlDbType.Int);
				storedProcedureCommand.Parameters["@ReturnVal"].Direction = ParameterDirection.ReturnValue;

				storedProcedureCommand.ExecuteNonQuery();
				storedProcedureCommand.Dispose();

				int parent    = 0;
				int role      = 0;
				int usr       = 0;
				int effective = 0;
				int grant     = 0;
				int mask      = 0;

				GrantsRow grants = null;

				bool existParent     = Convert.ToBoolean(storedProcedureCommand.Parameters["@parout_existparent_object"].Value);
				bool protectedObject = Convert.ToBoolean(storedProcedureCommand.Parameters["@parout_protected_object"].Value);
				
				//USER
				if (!isRole && storedProcedureCommand.Parameters["@parout_thereis_usrgrant"].Value != DBNull.Value)
				{
					usr = Convert.ToInt32(storedProcedureCommand.Parameters["@parout_thereis_usrgrant"].Value);
					grant = Convert.ToInt32(storedProcedureCommand.Parameters["@parout_usr_grant"].Value);
					mask  = Convert.ToInt32(storedProcedureCommand.Parameters["@parout_usr_inheritmask"].Value);
				}
				if (grantsArrayList != null && grantsArrayList.Count > 0)
				{
					for(int i=0; i < grantsArrayList.Count; i++)
					{
						if (grantsArrayList[i] == null || !(grantsArrayList[i] is GrantsRow))
							continue;
						((GrantsRow)grantsArrayList[i]).SetUserIconState(usr != 0, protectedObject, grant, mask);
					}
				}

				//ROLE
				if (storedProcedureCommand.Parameters["@parout_thereis_rolegrant"].Value != DBNull.Value)
					role = Convert.ToInt32(storedProcedureCommand.Parameters["@parout_thereis_rolegrant"].Value);	
				grant = Convert.ToInt32(storedProcedureCommand.Parameters["@parout_role_grant"].Value);
				mask  = Convert.ToInt32(storedProcedureCommand.Parameters["@parout_role_inheritmask"].Value);
				if (grantsArrayList != null && grantsArrayList.Count > 0)
				{
					for(int i=0; i < grantsArrayList.Count; i++)
					{
						if (grantsArrayList[i] == null || !(grantsArrayList[i] is GrantsRow))
							continue;
						grants = (GrantsRow) grantsArrayList[i];
						((GrantsRow)grantsArrayList[i]).SetRoleIconState(role != 0, protectedObject, grant, mask, grants.Mask, isRole);
					}
				}

				//PARENT
				if(storedProcedureCommand.Parameters["@parout_thereis_parentgrant"].Value != DBNull.Value  )
					parent = Convert.ToInt32(storedProcedureCommand.Parameters["@parout_thereis_parentgrant"].Value);
				grant = Convert.ToInt32(storedProcedureCommand.Parameters["@parout_parent_grant"].Value);
				mask  = Convert.ToInt32(storedProcedureCommand.Parameters["@parout_parent_inheritmask"].Value);
				if (grantsArrayList != null && grantsArrayList.Count > 0)
				{
					for(int i=0; i < grantsArrayList.Count; i++)
					{
						if (grantsArrayList[i] == null || !(grantsArrayList[i] is GrantsRow))
							continue;
						((GrantsRow)grantsArrayList[i]).SetInheritIconState(parent != 0, existParent, grant, mask);
					}
				}

				//EFFETTIVO
				if (storedProcedureCommand.Parameters["@parout_thereis_totalgrant"].Value != DBNull.Value)
					effective = Convert.ToInt32(storedProcedureCommand.Parameters["@parout_thereis_totalgrant"].Value);
				
				grant = Convert.ToInt32(storedProcedureCommand.Parameters["@parout_total_grant"].Value);
				mask  = Convert.ToInt32(storedProcedureCommand.Parameters["@parout_total_inheritmask"].Value);

				
				if (grantsArrayList != null && grantsArrayList.Count > 0)
				{
					for(int i=0; i < grantsArrayList.Count; i++)
					{
						if (grantsArrayList[i] == null || !(grantsArrayList[i] is GrantsRow))
							continue;
						
						grants = (GrantsRow) grantsArrayList[i];
						((GrantsRow)grantsArrayList[i]).SetTotalIconState(effective != 0, protectedObject, grant, mask, grants.Mask);
												
					}
				}
			}
			catch (SqlException err)
			{
				if (storedProcedureCommand != null)
					storedProcedureCommand.Dispose();
				DiagnosticViewer.ShowError(Strings.Error, err.Message, err.Procedure, err.Number.ToString(), SecurityConstString.SecurityAdminPlugIn);
			}
		}
		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che restituisce un ArrayList contente i tipi di permessi su 
		/// un tipo  di oggetto
		/// </summary>
		/// <param name="objectType"></param>
		/// <param name="sqlOSLConnection"></param>
		/// <returns></returns>
		static public ArrayList GetTypesGrants(int objectType, SqlConnection sqlOSLConnection)
		{
			if (sqlOSLConnection == null || sqlOSLConnection.State != ConnectionState.Open)
				return null;
			
			
			string selectGrantsQuery = @"SELECT GrantMask, TypeId, GrantName FROM MSD_ObjectTypeGrants WHERE TypeId = " + objectType.ToString();

			SqlCommand mySqlCommand = null;
			SqlDataReader myReader = null;
			try
			{
				mySqlCommand = new SqlCommand(selectGrantsQuery, sqlOSLConnection);

				myReader = mySqlCommand.ExecuteReader();

				ArrayList dbTypeGrants = new ArrayList();

				while (myReader.Read())
				    dbTypeGrants.Add(new GrantsRow(Convert.ToInt32(myReader["GrantMask"]), Convert.ToInt32(myReader["TypeId"]), GrantsString.GetGrantDescription(myReader["GrantName"].ToString())));
					
				return (dbTypeGrants.Count > 0) ? dbTypeGrants : null;
			}
			catch (SqlException)
			{
				return null;
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

		#region InsertObjectInDB

		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che inserisce un oggetto nel DataBase
		/// </summary>
		/// <param name="node"></param>
		/// <param name="sqlOSLConnection"></param>
		/// <returns></returns>
		static public bool InsertObjectInDB(MenuXmlNode node, SqlConnection sqlOSLConnection)
		{
			int				parentId	= 0;
			SqlDataReader	myReader	= null;

			if (!node.IsCommand)
				return false;
				
			MenuXmlNode	commandParentNode = (node.IsExternalItem) ? node.GetParentNode() : null;

            int commandTypeFromDataBase = CommonObjectTreeFunction.GetObjectTypeId(node, sqlOSLConnection); 
		
			SqlCommand selectObjectIdSqlCommand = null;
			SqlCommand insertObjectIdSqlCommand = null;
			try			
			{
				string selectObjectIdQuery = @"SELECT ObjectId FROM MSD_Objects WHERE NameSpace = @NameSpace";
				
				selectObjectIdSqlCommand = new SqlCommand(selectObjectIdQuery, sqlOSLConnection);
				selectObjectIdSqlCommand.Parameters.Add("@NameSpace", SqlDbType.NVarChar, 512, "NameSpace");

				if (commandParentNode != null  && commandParentNode.ItemObject != null && commandParentNode.ItemObject != String.Empty && 
					!(String.Compare(node.ExternalItemType, SecurityType.Report.ToString(), true, CultureInfo.InvariantCulture) == 0))
				{
					//Cerco il padre
					selectObjectIdSqlCommand.Parameters["@NameSpace"].Value = commandParentNode.ItemObject;
					myReader = selectObjectIdSqlCommand.ExecuteReader();
					if (myReader != null)
					{
						if (myReader.Read())
							parentId = Convert.ToInt32(myReader["ObjectId"]);
						myReader.Close();
					}
					selectObjectIdSqlCommand.Dispose();
				}

				string insertObjectQuery =@"INSERT INTO MSD_Objects
											(TypeId, NameSpace, ParentId) VALUES
											(@TypeId, @NameSpace, @ParentId)";
				
				insertObjectIdSqlCommand =  new SqlCommand(insertObjectQuery,  sqlOSLConnection);
				insertObjectIdSqlCommand.Parameters.Add("@TypeId", SqlDbType.Int, 4, "TypeId");
				insertObjectIdSqlCommand.Parameters.Add("@NameSpace",  SqlDbType.NVarChar, 512, "NameSpace");
				insertObjectIdSqlCommand.Parameters.Add("@ParentId", SqlDbType.Int, 4, "ParentId");


				insertObjectIdSqlCommand.Parameters["@ParentId"].Value  = parentId;
				insertObjectIdSqlCommand.Parameters["@TypeId"].Value	= commandTypeFromDataBase;
				insertObjectIdSqlCommand.Parameters["@NameSpace"].Value	= node.ItemObject;
				
				insertObjectIdSqlCommand.ExecuteNonQuery();
				
				insertObjectIdSqlCommand.Dispose();

				return true;
			}
			catch (SqlException e)
			{
				Debug.Fail("Exception thrown in CommonObjectTreeFunction.InsertObjectInDB: " + e.Message);

				if (myReader != null && myReader.IsClosed) 
					myReader.Close();
				if (insertObjectIdSqlCommand != null) 
					insertObjectIdSqlCommand.Dispose();
				if (selectObjectIdSqlCommand != null)
					selectObjectIdSqlCommand.Dispose();
				return false;
			}
		}

        //---------------------------------------------------------------------
        static public bool InsertObjectInDB(NameSpace aNameSpace, int type, SqlConnection sqlOSLConnection)
        {
            return InsertObjectInDB(aNameSpace.ToString(), type, sqlOSLConnection);
        }
        //---------------------------------------------------------------------
        static public bool InsertObjectInDB(string aNameSpace, int type, SqlConnection sqlOSLConnection, int parentID = 0)
        {
            SqlDataReader myReader = null;

            SqlCommand insertObjectIdSqlCommand = null;

            try
            {
                string insertObjectQuery = @"INSERT INTO MSD_Objects
										(TypeId, NameSpace, ParentId) VALUES
										(@TypeId, @NameSpace, @ParentId)";

                insertObjectIdSqlCommand = new SqlCommand(insertObjectQuery, sqlOSLConnection);
                insertObjectIdSqlCommand.Parameters.Add("@TypeId", SqlDbType.Int, 4, "TypeId");
                insertObjectIdSqlCommand.Parameters.Add("@NameSpace", SqlDbType.NVarChar, 512, "NameSpace");
                insertObjectIdSqlCommand.Parameters.Add("@ParentId", SqlDbType.Int, 4, "ParentId");


                insertObjectIdSqlCommand.Parameters["@ParentId"].Value = parentID;
                insertObjectIdSqlCommand.Parameters["@TypeId"].Value = CommonObjectTreeFunction.GetObjectTypeId(type, sqlOSLConnection);
                insertObjectIdSqlCommand.Parameters["@NameSpace"].Value = aNameSpace;

                insertObjectIdSqlCommand.ExecuteNonQuery();

                insertObjectIdSqlCommand.Dispose();

                return true;
            }
            catch (SqlException e)
            {
                Debug.Fail("Exception thrown in CommonObjectTreeFunction.InsertObjectInDB: " + e.Message);

                if (myReader != null && myReader.IsClosed)
                    myReader.Close();
                if (insertObjectIdSqlCommand != null)
                    insertObjectIdSqlCommand.Dispose();
                return false;
            }
        }

		//---------------------------------------------------------------------
		public static int GetApplicationUserID(string easyLookUserName, SqlConnection tmpConnection)
		{
			if (tmpConnection.State != ConnectionState.Open)
				tmpConnection.Open();
			int id = -1;
			string sSelect = "SELECT LoginId FROM MSD_Logins WHERE Login= @Login";

			SqlCommand mySqlCommand = new SqlCommand(sSelect, tmpConnection);
            mySqlCommand.Parameters.AddWithValue("@Login", easyLookUserName);
			SqlDataReader myReader  = mySqlCommand.ExecuteReader();
			if (!myReader.Read())
				id =  -1;
			else
				id = Convert.ToInt32(myReader["LoginId"]);
			
			myReader.Close();
			mySqlCommand.Dispose();
			return id ;
		}

		
	}
	//=========================================================================
	/// <summary>
	/// Classe che gestisce le operazioni di Autocomepletamento e regole di congruità
	/// sui Grants
	/// </summary>
	public class ImportExportRole
	{
		#region Privated Data Member 
        private ArrayList       objects = null;
		private IList			roles		= null;
		private string			filePath	= string.Empty;
		private	bool			valid;
		private	string			parsingError;
		private string			connectionString = string.Empty;
		private IBaseModuleInfo	parentModuleInfo;
		#endregion

		#region Properties

		public	string		FilePath		{ get { return filePath; } }
		public	bool		Valid			{ get { return valid; } }
		public	string		ParsingError	{ get { return parsingError; } }
		public	IList		Roles			{ get { return roles; } }
        public ArrayList Objects { get { return objects; } set { objects = value; } }
		#endregion

		#region Costruttors
		//---------------------------------------------------------------------
		public ImportExportRole(string aFilePath, IBaseModuleInfo	aParentModuleInfo)
		{
			if (aFilePath == null || aFilePath.Length == 0)
			{
				Debug.Fail("Error in DocumentsObjectInfo");
			}

			filePath			= aFilePath;
			valid				= true;
			parsingError		= string.Empty;
			parentModuleInfo	= aParentModuleInfo;
		}

        public ImportExportRole() 
        {

        }
        //---------------------------------------------------------------------
        public ImportExportRole(string aFilePath) : this(aFilePath, null)
        {

        }
        #endregion
        
        #region Parser Functions
        /// <summary>
        /// Legge il file e crea gli array di document e clientDocument in memoria
        /// </summary>
        /// <returns>true se la lettura ha avuto successo</returns>
        //---------------------------------------------------------------------
        public bool Parse()
		{

			if (!File.Exists(filePath))
				return false;

            LocalizableXmlDocument roleDocument = null;
            XmlElement root = null;

            try
            {
                if (parentModuleInfo != null)
                {
                    if (!File.Exists(filePath))
                        return false;

                    roleDocument = new LocalizableXmlDocument
														    (
														    parentModuleInfo.ParentApplicationInfo.Name,
														    parentModuleInfo.Name,
														    parentModuleInfo.PathFinder
														    );


                    //leggo il file
                    roleDocument.Load(filePath);
                    root = roleDocument.DocumentElement;
                }
                else
                {
                    XmlDocument doc = new XmlDocument();
                    //leggo il file
                    doc.Load(filePath);
                    root = doc.DocumentElement;
                }

				if (root != null)
				{
					//array dei Role
					XmlNodeList roleElements = ((XmlElement) root).GetElementsByTagName("Role");
					if (roleElements == null)
						return true;
			
					ParseRole(roleElements);

                    //array degli Obbetti
                    XmlNodeList objectElements = ((XmlElement)root).GetElementsByTagName("Object");
                    if (objectElements == null)
                        return true;

                    ParseObjectsForRolesImport(objectElements);

				}
				
			}
			catch(XmlException e)
			{
				Debug.Fail(e.Message);
				valid = false;
				parsingError = e.Message;
				return false;
			}
			catch(Exception err)
			{
				Debug.Fail(err.Message);
				valid = false;
				parsingError = err.Message;
				return false;
			}
			return true;
		}

		//---------------------------------------------------------------------
		private bool ParseRole(XmlNodeList roleElements)
		{
			if (roleElements == null)
				return false;

			//inizializzo l'array dei documenti
			if (roles == null)
				roles = new ArrayList();
			else
				roles.Clear();

			string	name		= string.Empty;
			string	description	= string.Empty;
			bool	disabled	= false;
            bool    readOnly    = false;

			string	nameSpace		= string.Empty;
			string	parentNameSpace = string.Empty;

			int		typeId		= 0;
			int		grant		= 0;
			int		inheritMask	= 0;

			Role	role		= null;
			Grant	grantobj	= null;

			//scorro i ruoli
			foreach (XmlElement roleElement in roleElements)
			{
				//namespace del documento
				name		= roleElement.GetAttribute("localize");
				description	= roleElement.GetElementsByTagName("Description")[0].InnerText;
				disabled	= Convert.ToBoolean(roleElement.GetElementsByTagName("Disabled")[0].InnerText);

                if (roleElement.GetElementsByTagName("ReadOnly")[0] != null)
                    readOnly = Convert.ToBoolean(roleElement.GetElementsByTagName("ReadOnly")[0].InnerText);
                else
                    readOnly = false;

				//creo l'oggetto che tiene le info raccolte
                role = new Role(name, description, disabled, DefaultAdvancedRolesType.FromFile, readOnly);
				roles.Add(role);

				//Cerco il nodo <Grants>
				XmlNodeList grantsNodes = roleElement.GetElementsByTagName("Grants");
				if (grantsNodes != null && grantsNodes.Count == 1)
				{
					//array dei documenti
					XmlNodeList grantElements = ((XmlElement) grantsNodes[0]).GetElementsByTagName("Grant");
					if (grantElements == null)
						return false;
			
					if (role.Grats == null)
						role.Grats = new ArrayList();

					//Parso i grant di quel Ruolo
					foreach (XmlElement grantElement in grantElements)
					{
						nameSpace	= grantElement.GetElementsByTagName("NameSpace")[0].InnerText;
						typeId		= Convert.ToInt32(grantElement.GetElementsByTagName("TypeId")[0].InnerText);
						grant		= Convert.ToInt32(grantElement.GetElementsByTagName("GrantValue")[0].InnerText);
						inheritMask = Convert.ToInt32(grantElement.GetElementsByTagName("InheritMask")[0].InnerText);
						
						grantobj = new Grant(grant, inheritMask, nameSpace, typeId);
						role.Grats.Add(grantobj);
					}
				}
			}

			return true;
		}

        //---------------------------------------------------------------------
        private bool ParseObjectsForRolesImport(XmlNodeList objectElements)
        {
            if (objectElements == null)
                return false;

            //inizializzo l'array dei documenti
            if (objects == null)
                objects = new ArrayList();
            else
                objects.Clear();

            //string name = string.Empty;
            //string description = string.Empty;
            //bool disabled = false;

            string nameSpace = string.Empty;
            string nameSpaceParent = string.Empty;

            int typeId = 0;
            int typeIdParent = 0;

            ObjectForGrantsImport obj;
            foreach (XmlElement objectElement in objectElements)
            {
                ////namespace del documento
                //name = roleElement.GetAttribute("localize");
                //description = roleElement.GetElementsByTagName("Description")[0].InnerText;
                //disabled = Convert.ToBoolean(roleElement.GetElementsByTagName("Disabled")[0].InnerText);

                ////creo l'oggetto che tiene le info raccolte
                //role = new Role(name, description, disabled, DefaultAdvancedRolesType.FromFile);
                //roles.Add(role);

                ////Cerco il nodo <Grants>
                //XmlNodeList grantsNodes = roleElement.GetElementsByTagName("Grants");
                //if (grantsNodes != null && grantsNodes.Count == 1)
                //{
                //    //array dei documenti
                //    XmlNodeList grantElements = ((XmlElement)grantsNodes[0]).GetElementsByTagName("Grant");
                //    if (grantElements == null)
                //        return false;

                //    if (role.Grats == null)
                //        role.Grats = new ArrayList();

                //    //Parso i grant di quel Ruolo
                //    foreach (XmlElement grantElement in grantElements)
                //    {
                nameSpace = objectElement.GetElementsByTagName("NameSpace")[0].InnerText;
                typeId = Convert.ToInt32(objectElement.GetElementsByTagName("TypeID")[0].InnerText);
                nameSpaceParent = objectElement.GetElementsByTagName("NameSpaceParent")[0].InnerText;
                typeIdParent = Convert.ToInt32(objectElement.GetElementsByTagName("TypeIDParent")[0].InnerText);

                obj = new ObjectForGrantsImport(nameSpace, typeId, nameSpaceParent, typeIdParent);
                //        grant = Convert.ToInt32(grantElement.GetElementsByTagName("GrantValue")[0].InnerText);
                //        inheritMask = Convert.ToInt32(grantElement.GetElementsByTagName("InheritMask")[0].InnerText);

                //        grantobj = new Grant(grant, inheritMask, nameSpace, typeId);
                objects.Add(obj);
                //    }
               
            }

            return true;
        }
        #endregion

        //---------------------------------------------------------------------
        public Role GetRoleByName(string name)
		{
			foreach(Role role in roles)
			{
				if (string.Compare(role.RoleName, name, true , CultureInfo.InvariantCulture)== 0)
					return role;
			}

			return null;
		}
	}

    //=========================================================================
    public class Role
    {
        private string roleName = string.Empty;
        private string roleDescription = string.Empty;
        private bool disabled = false;
        private bool readOnly = false;
        private ArrayList grants = null;
        
        private int roleId = -1;
        private DefaultAdvancedRolesType roleType;
        private string roleNewName = string.Empty;

        public string RoleName { get { return roleName; } set { roleName = value; } }
        public string RoleNewName { get { return roleNewName; } set { roleNewName = value; } }
        public int RoleId { get { return roleId; } set { roleId = value; } }
        public string RoleDescription { get { return roleDescription; } set { roleDescription = value; } }
        public ArrayList Grats { get { return grants; } set { grants = value; } }
        public DefaultAdvancedRolesType RoleType { get { return roleType; } set { roleType = value; } }
        public bool Disabled { get { return disabled; } set { disabled = value; } }
        public bool ReadOnly { get { return readOnly; } set { readOnly = value; } }

        //---------------------------------------------------------------------
        public Role(string roleName, string roleDescription, bool disabled, DefaultAdvancedRolesType roleType, bool readOnly)
        {
            this.roleName = roleName;
            this.roleDescription = roleDescription;
            this.disabled = disabled;
            this.roleType = roleType;
            this.roleNewName = roleName;
            this.readOnly = readOnly;

            if (grants == null)
                grants = new ArrayList();
        }
    }

	//=========================================================================
	public class Grant
	{
		private int grantValue = 0;
		private int inheritMask = 0;
		private string  nameSpace = string.Empty ;
		private int typeId = 0;
		private int	parentId = 0;

		public int GrantValue	{ get{ return grantValue; }		set { grantValue = value;}}
		public int InheritMask	{ get{ return inheritMask; }	set { inheritMask = value;}}
		public string NameSpace { get{ return nameSpace; }		set { nameSpace = value;}}
		public int TypeId		{ get{ return typeId; }			set { typeId = value;}}
		public int ParentId		{ get{ return parentId; }		set { parentId = value;}}

		public Grant(int grantValue, int inheritMask, string nameSpace, int typeId)
		{
			this.grantValue = grantValue;
			this.inheritMask = inheritMask;
			this.nameSpace = nameSpace;
			this.typeId = typeId;
		}

		public Grant()
		{
		
		}

	}

    //=========================================================================
    public class ObjectForGrantsImport
    {

        private string  nameSpaceParent = string.Empty;
        private string  nameSpace = string.Empty;
        private int     typeId = 0;
        private int     typeIdParent = 0;

        public string NameSpaceParent { get { return nameSpaceParent; } set { nameSpaceParent = value; } }
        public string NameSpace { get { return nameSpace; } set { nameSpace = value; } }
        public int TypeId { get { return typeId; } set { typeId = value; } }
        public int TypeIdParent { get { return typeIdParent; } set { typeIdParent = value; } }

        public ObjectForGrantsImport(string nameSpace, int typeId,  string nameSpaceParent, int typeIdParent)
        {
            this.typeIdParent = typeIdParent;
            this.nameSpaceParent = nameSpaceParent;
            this.nameSpace = nameSpace;
            this.typeId = typeId;
        }

        public ObjectForGrantsImport()
        {

        }

    }

	//=========================================================================
	/// <summary>
	/// Classe che gestisce le operazioni di Autocomepletamento e regole di congruità
	/// sui Grants
	/// </summary>
	public class GrantsFunctions
	{
		#region Autocompletamento
		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che inserisce i grants che l'utente non ha selezionato.
		/// Se sei un Ruolo e hai un valore nella col Ruolo metto quello se no eredita
		/// tranne per il grant 'Silent Mode' che prende 'PERMETTI'.
		/// Lo stesso per gli Utenti con la colonna Utente
		/// </summary>
		/// <param name="dataTable"></param>
		/// <param name="grants"></param>
		/// <param name="inheritMask"></param>
		/// <param name="arrayListGrants"></param>
		public static void AutoCompleteGrants(ref DataTable dataTable, ref int grants, ref int inheritMask, 
												ArrayList arrayListGrants)
		{
			
			for(int i =0; i < arrayListGrants.Count; i++)
			{
				if (arrayListGrants[i] == null || !(arrayListGrants[i] is GrantsRow))
					continue;
				
				GrantsRow rowGrants	= (GrantsRow)arrayListGrants[i];
				DataRow dr	= dataTable.Rows[i];
				GrantsRow.IconState	iconState = (GrantsRow.IconState)dr[securityGrants.Assign];
				
				switch (iconState)
				{
					case GrantsRow.IconState.IconAllowed:
						grants      = Bit.SetUno(grants, rowGrants.Mask);
						inheritMask = Bit.SetZero(inheritMask, rowGrants.Mask);
						break;
					case GrantsRow.IconState.IconForbidden:
						grants      = Bit.SetZero(grants, rowGrants.Mask);
						inheritMask = Bit.SetZero(inheritMask, rowGrants.Mask);
						break;
					case GrantsRow.IconState.IconInherit:
						grants      = Bit.SetZero(grants, rowGrants.Mask);
						inheritMask = Bit.SetUno(inheritMask, rowGrants.Mask);
						break;
					case GrantsRow.IconState.IconNotExist:
						dataTable.Rows[i].BeginEdit();
						if (Convert.ToInt32(dataTable.Rows[i][securityGrants.GrantMask])==512)
						{
							dataTable.Rows[i][securityGrants.Assign] = GrantsRow.IconState.IconAllowed;
							grants      = Bit.SetUno(grants, rowGrants.Mask);
							inheritMask = Bit.SetZero(inheritMask, rowGrants.Mask);
						}
						else
						{
							dataTable.Rows[i][securityGrants.Assign] = GrantsRow.IconState.IconInherit;
							grants      = Bit.SetZero(grants, rowGrants.Mask);
							inheritMask = Bit.SetUno(inheritMask, rowGrants.Mask);
						}
						dataTable.Rows[i].EndEdit();
						break;
				}
			}
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che autocompleta i grants in base ai valori presenti per il
		/// Ruolo
		/// </summary>
		/// <param name="dataTable"></param>
		/// <param name="grants"></param>
		/// <param name="inheritMask"></param>
		/// <param name="arrayListGrants"></param>
		public static void AutoCompleteWhithRoleValues(ref DataTable dataTable, ref int grants, ref int inheritMask, ArrayList arrayListGrants)
		{
			//Completo le celle vuote
			for(int i =0; i < arrayListGrants.Count; i++)
			{
				if (arrayListGrants[i] == null || !(arrayListGrants[i] is GrantsRow))
					continue;

				GrantsRow rowGrants	= (GrantsRow)arrayListGrants[i];

				DataRow dr	= dataTable.Rows[i];
				GrantsRow.IconState iconState = (GrantsRow.IconState)dr[securityGrants.Assign];
				
				if(iconState  == GrantsRow.IconState.IconNotExist)
				{
					dataTable.Rows[i].BeginEdit();
					dataTable.Rows[i][securityGrants.Assign] = dataTable.Rows[i][securityGrants.Role];
					dataTable.Rows[i].EndEdit();
				}
			}
			//Calcolo i Grants da dare in pasto alla s.p.
			AutoCompleteGrants(ref dataTable, ref grants, ref inheritMask, arrayListGrants);
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che autocompleta i permessi in base ai valori presenti per
		/// l'Utente
		/// </summary>
		/// <param name="dataTable"></param>
		/// <param name="grants"></param>
		/// <param name="inheritMask"></param>
		/// <param name="arrayListGrants"></param>
		public static void AutoCompleteWhithUserValues(ref DataTable dataTable, ref int grants, ref int inheritMask, ArrayList arrayListGrants)
		{
			//Completo le celle vuote
			for(int i =0; i<arrayListGrants.Count; i++)
			{
				if (arrayListGrants[i] == null || !(arrayListGrants[i] is GrantsRow))
					continue;

				GrantsRow rowGrants	= (GrantsRow)arrayListGrants[i];

				DataRow dr	= dataTable.Rows[i];
				GrantsRow.IconState iconState = (GrantsRow.IconState)dr[securityGrants.Assign];
				
				if(iconState  == GrantsRow.IconState.IconNotExist)
				{
					dataTable.Rows[i].BeginEdit();
					dataTable.Rows[i][securityGrants.Assign] = dataTable.Rows[i][securityGrants.User];
					dataTable.Rows[i].EndEdit();
				}
			}

			//Calcolo i Grants da dare in pasto alla s.p.
			AutoCompleteGrants(ref dataTable, ref grants, ref inheritMask, arrayListGrants);
		}
		
		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che applica le varie regole di autocompletamento dei permessi
		/// </summary>
		/// <param name="oldDataTable"></param>
		/// <param name="grants"></param>
		/// <param name="inheritMask"></param>
		/// <param name="arrayTypeGrants"></param>
		/// <param name="isUser"></param>
		public static void ApplyGrantsRules(ref DataTable oldDataTable, ref int grants, ref int inheritMask, 
			ArrayList arrayTypeGrants, bool isUser)
		{
			GrantsRow.IconState executeIconState = GrantsFunctions.FindGrant(oldDataTable, arrayTypeGrants, 1);
			
			if (executeIconState == GrantsRow.IconState.IconForbidden) 
				GrantsFunctions.SetForbidden(ref oldDataTable, ref grants, ref inheritMask, arrayTypeGrants);
			else
			{
				if (GrantsFunctions.FindGrant(oldDataTable, arrayTypeGrants, 1024) == GrantsRow.IconState.IconAllowed)
					GrantsFunctions.SetAllowExtendedBrowse(ref oldDataTable, ref grants, ref inheritMask, arrayTypeGrants);
				else if (GrantsFunctions.FindGrant(oldDataTable, arrayTypeGrants, 1024) == GrantsRow.IconState.IconForbidden)
					GrantsFunctions.SetForbiddenExtendedBrowse(ref oldDataTable, ref grants, ref inheritMask, arrayTypeGrants);

				if (GrantsFunctions.FindGrant(oldDataTable, arrayTypeGrants, 16) ==  GrantsRow.IconState.IconForbidden)
					GrantsFunctions.SetForbiddenBrowse(ref oldDataTable, ref grants, ref inheritMask, arrayTypeGrants);	
			}
		
			//Controllo se esiste qualche valore x la colonna ruolo/utente
			if (!isUser)
			{
				if ((GrantsRow.IconState)oldDataTable.Rows[0][securityGrants.Role] != GrantsRow.IconState.IconNotAssigned)
				{
					//Setto i valori precedenti della colonna ruolo
					GrantsFunctions.AutoCompleteWhithRoleValues(ref oldDataTable, ref grants, ref inheritMask, arrayTypeGrants);
					executeIconState = GrantsFunctions.FindExecuteGrant(oldDataTable, arrayTypeGrants);
					if (executeIconState == GrantsRow.IconState.IconForbidden) 
						GrantsFunctions.SetForbidden(ref oldDataTable, ref grants, ref inheritMask, arrayTypeGrants);
					else
					{
						if (GrantsFunctions.FindGrant(oldDataTable, arrayTypeGrants, 1024) == GrantsRow.IconState.IconAllowed)
							GrantsFunctions.SetAllowExtendedBrowse(ref oldDataTable, ref grants, ref inheritMask, arrayTypeGrants);
						else if (GrantsFunctions.FindGrant(oldDataTable, arrayTypeGrants, 1024) == GrantsRow.IconState.IconForbidden)
							GrantsFunctions.SetForbiddenExtendedBrowse(ref oldDataTable, ref grants, ref inheritMask, arrayTypeGrants);

						if (GrantsFunctions.FindGrant(oldDataTable, arrayTypeGrants, 16) ==  GrantsRow.IconState.IconForbidden)
							GrantsFunctions.SetForbiddenBrowse(ref oldDataTable, ref grants, ref inheritMask, arrayTypeGrants);
				
					}
				}
				else
					GrantsFunctions.AutoCompleteGrants(ref oldDataTable, ref grants, ref inheritMask, arrayTypeGrants);
			}
			else
			{
				if ((GrantsRow.IconState)oldDataTable.Rows[0][securityGrants.User] != GrantsRow.IconState.IconNotAssigned)
				{
					//Setto i valori precedenti della colonna ruolo
					GrantsFunctions.AutoCompleteWhithUserValues(ref oldDataTable, ref grants, ref inheritMask, arrayTypeGrants);
					executeIconState = GrantsFunctions.FindExecuteGrant(oldDataTable, arrayTypeGrants);
					if (executeIconState == GrantsRow.IconState.IconForbidden) 
						GrantsFunctions.SetForbidden(ref oldDataTable, ref grants, ref inheritMask, arrayTypeGrants);
					else
					{
						if (GrantsFunctions.FindGrant(oldDataTable, arrayTypeGrants, 1024) == GrantsRow.IconState.IconAllowed)
							GrantsFunctions.SetAllowExtendedBrowse(ref oldDataTable, ref grants, ref inheritMask, arrayTypeGrants);
						else if (GrantsFunctions.FindGrant(oldDataTable, arrayTypeGrants, 1024) == GrantsRow.IconState.IconForbidden)
							GrantsFunctions.SetForbiddenExtendedBrowse(ref oldDataTable, ref grants, ref inheritMask, arrayTypeGrants);

						if (GrantsFunctions.FindGrant(oldDataTable, arrayTypeGrants, 16) ==  GrantsRow.IconState.IconForbidden)
							GrantsFunctions.SetForbiddenBrowse(ref oldDataTable, ref grants, ref inheritMask, arrayTypeGrants);
				
					}
				}
				GrantsFunctions.AutoCompleteGrants(ref oldDataTable, ref grants, ref inheritMask, arrayTypeGrants);	
			}
		}
		#endregion

		//----------------------------------------------------------------------
		public static bool isEBdevelopment(int userId, SqlConnection sqlConnection, int companyId)
		{
			if (sqlConnection == null || sqlConnection.State != ConnectionState.Open)
				return false;

			SqlCommand mySqlCommand = null;
			SqlDataReader myReader = null;

            string sSelect = @"SELECT * FROM MSD_CompanyLogins  where loginid = " + userId + " and EBDeveloper = 1 and companyid = " + companyId;
			try
			{
				using (mySqlCommand = new SqlCommand(sSelect, sqlConnection))
				{
					using (myReader = mySqlCommand.ExecuteReader())
					{
						if (myReader.Read())
							return true;
					}
				}
			}
			catch (Exception ex )
			{
				Debug.WriteLine(ex.ToString());
			}

			finally
			{
				if (myReader != null && !myReader.IsClosed)
					myReader.Close();
				if (mySqlCommand != null)
					mySqlCommand.Dispose();
			}
			return false;

		}

		//----------------------------------------------------------------------
        public static bool isAdmin(int userId, SqlConnection sqlConnection, int companyId)
		{
			if (sqlConnection == null || sqlConnection.State != ConnectionState.Open)
				return false;

			SqlCommand mySqlCommand = null;
			SqlDataReader myReader = null;

            string sSelect = @"SELECT * FROM MSD_CompanyLogins  where loginid = " + userId + " and Admin = 1 and companyid = " + companyId;
			try
			{
				mySqlCommand = new SqlCommand(sSelect, sqlConnection);
				myReader = mySqlCommand.ExecuteReader();
				if (myReader.Read())
					return true;
			}
			catch (SqlException)
			{
			}
			finally
			{
				if (myReader != null && !myReader.IsClosed)
					myReader.Close();
				if (mySqlCommand != null)
					mySqlCommand.Dispose();
			}
			return false;

		}

		//----------------------------------------------------------------------
		public static bool isReportOwner(int userId, SqlConnection sqlConnection, int companyId)
		{
			if (sqlConnection == null || sqlConnection.State != ConnectionState.Open)
				return false;

			SqlCommand mySqlCommand = null;
			SqlDataReader myReader = null;

			string sSelect = @"select * from msd_objectgrants 
							join [MSD_Objects]  on [MSD_ObjectGrants].ObjectId = msd_objects.ObjectId
							where
							msd_objects.NameSpace = 'Framework.TbWoormViewer.TbWoormViewer.IsUserReportsDeveloper' and  LOGInid =" + userId + " and companyid = " + companyId;
			try
			{
				mySqlCommand = new SqlCommand(sSelect, sqlConnection);
				myReader = mySqlCommand.ExecuteReader();
				if (myReader.Read())
					return true;
			}
			catch (SqlException)
			{
			}
			finally
			{
				if (myReader != null && !myReader.IsClosed)
					myReader.Close();
				if (mySqlCommand != null)
					mySqlCommand.Dispose();
			}
			return false;

		}
		/// <summary>
		/// Estrae il grant di 'ESECUZIONE' dall'array dei grant
		/// </summary>
		/// <param name="dataTable"></param>
		/// <param name="arrayListGrants"></param>
		/// <returns></returns>
		public static GrantsRow.IconState FindExecuteGrant(DataTable dataTable, ArrayList arrayListGrants)
		{
			for(int i =0; i < arrayListGrants.Count; i++)
			{
				if (arrayListGrants[i] == null || !(arrayListGrants[i] is GrantsRow))
					continue;

				GrantsRow rowGrants	= (GrantsRow)arrayListGrants[i];
				DataRow dr	= dataTable.Rows[i];
				GrantsRow.IconState iconState = (GrantsRow.IconState)dr[securityGrants.Assign];
				if (Convert.ToInt32(dr[securityGrants.GrantMask]) == 1)
					return iconState;
			}
			return GrantsRow.IconState.IconAllowed;

		}

		//---------------------------------------------------------------------
		public static GrantsRow.IconState FindExtendedBrowseGrant(DataTable dataTable, ArrayList arrayListGrants)
		{
			for(int i =0; i < arrayListGrants.Count; i++)
			{
				if (arrayListGrants[i] == null || !(arrayListGrants[i] is GrantsRow))
					continue;

				GrantsRow rowGrants	= (GrantsRow)arrayListGrants[i];
				DataRow dr	= dataTable.Rows[i];
				GrantsRow.IconState iconState = (GrantsRow.IconState)dr[securityGrants.Assign];
				if (Convert.ToInt32(dr[securityGrants.GrantMask]) == 1024)
					return iconState;
			}
			return GrantsRow.IconState.IconAllowed;

		}
		//---------------------------------------------------------------------
		public static GrantsRow.IconState FindGrant(DataTable dataTable, ArrayList arrayListGrants, int grantValue)
		{
			for(int i =0; i < arrayListGrants.Count; i++)
			{
				if (arrayListGrants[i] == null || !(arrayListGrants[i] is GrantsRow))
					continue;

				GrantsRow rowGrants	= (GrantsRow)arrayListGrants[i];
				DataRow dr	= dataTable.Rows[i];
				GrantsRow.IconState iconState = (GrantsRow.IconState)dr[securityGrants.Assign];
				if (Convert.ToInt32(dr[securityGrants.GrantMask]) == grantValue)
					return iconState;
			}
			return GrantsRow.IconState.IconNotExist;

		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Se l'Utente ha messo 'NEGATO' sul permesso 'ESEGUI' devo mettere anche tutti gli 
		/// altri a 'Negato' tranne la 'MODALITA SILENTE' che prende 'PERMETTI'
		/// </summary>
		/// <param name="dataTable"></param>
		/// <param name="grants"></param>
		/// <param name="inheritMask"></param>
		/// <param name="arrayListGrants"></param>
		public static void SetForbidden(ref DataTable dataTable, ref int grants, ref int inheritMask, ArrayList arrayListGrants)
		{
			grants = 0;
			inheritMask =0 ;

			for(int i =0; i<arrayListGrants.Count; i++)
			{
				if (arrayListGrants[i] == null || !(arrayListGrants[i] is GrantsRow))
					continue;

				GrantsRow rowGrants	= (GrantsRow)arrayListGrants[i];
				DataRow dr	= dataTable.Rows[i];
				GrantsRow.IconState iconState = (GrantsRow.IconState)dr[securityGrants.Assign];

				if (Convert.ToInt32(dataTable.Rows[i][securityGrants.GrantMask])==512) 
				{
					switch (iconState)
					{
						case GrantsRow.IconState.IconAllowed:
							grants      = Bit.SetUno(grants, rowGrants.Mask);
							inheritMask = Bit.SetZero(inheritMask, rowGrants.Mask);
							break;
						case GrantsRow.IconState.IconForbidden:
							grants      = Bit.SetZero(grants, rowGrants.Mask);
							inheritMask = Bit.SetZero(inheritMask, rowGrants.Mask);
							break;
						case GrantsRow.IconState.IconInherit:
							grants      = Bit.SetZero(grants, rowGrants.Mask);
							inheritMask = Bit.SetUno(inheritMask, rowGrants.Mask);
							break;
						case GrantsRow.IconState.IconNotExist:
							dataTable.Rows[i].BeginEdit();
							dataTable.Rows[i][securityGrants.Assign] = GrantsRow.IconState.IconAllowed;
							grants      = Bit.SetUno(grants, rowGrants.Mask);
							inheritMask = Bit.SetZero(inheritMask, rowGrants.Mask);
							dataTable.Rows[i].EndEdit();
							break;
					}
					continue;
				}
				dataTable.Rows[i].BeginEdit();
				dataTable.Rows[i][securityGrants.Assign] = GrantsRow.IconState.IconForbidden;
				grants      = Bit.SetZero(grants, rowGrants.Mask);
				inheritMask = Bit.SetZero(inheritMask, rowGrants.Mask);
				dataTable.Rows[i].EndEdit();
			}
		}
		
		//---------------------------------------------------------------------
		public static void SetAllowExtendedBrowse(ref DataTable dataTable, ref int grants, ref int inheritMask, ArrayList arrayListGrants)
		{

			for(int i =0; i<arrayListGrants.Count; i++)
			{
				if (arrayListGrants[i] == null || !(arrayListGrants[i] is GrantsRow))
					continue;

				GrantsRow rowGrants	= (GrantsRow)arrayListGrants[i];
				DataRow dr	= dataTable.Rows[i];
				GrantsRow.IconState iconState = (GrantsRow.IconState)dr[securityGrants.Assign];

				if (Convert.ToInt32(dataTable.Rows[i][securityGrants.GrantMask])== 16) 
				{
					dataTable.Rows[i].BeginEdit();
					dataTable.Rows[i][securityGrants.Assign] = GrantsRow.IconState.IconAllowed;
					grants      = Bit.SetUno(grants, rowGrants.Mask);
					inheritMask = Bit.SetZero(inheritMask, rowGrants.Mask);
					dataTable.Rows[i].EndEdit();
				}
				
			}
		}

		//---------------------------------------------------------------------
		public static void SetForbiddenExtendedBrowse(ref DataTable dataTable, ref int grants, ref int inheritMask, ArrayList arrayListGrants)
		{
			for(int i =0; i<arrayListGrants.Count; i++)
			{
				if (arrayListGrants[i] == null || !(arrayListGrants[i] is GrantsRow))
					continue;

				GrantsRow rowGrants	= (GrantsRow)arrayListGrants[i];
				DataRow dr	= dataTable.Rows[i];
				GrantsRow.IconState iconState = (GrantsRow.IconState)dr[securityGrants.Assign];

				if (Convert.ToInt32(dataTable.Rows[i][securityGrants.GrantMask])== 64) 
				{
					dataTable.Rows[i].BeginEdit();
					dataTable.Rows[i][securityGrants.Assign] = GrantsRow.IconState.IconForbidden;
					grants      = Bit.SetZero(grants, rowGrants.Mask);
					inheritMask = Bit.SetZero(inheritMask, rowGrants.Mask);
					dataTable.Rows[i].EndEdit();
				}
				
			}
		}
		//---------------------------------------------------------------------
		public static void SetForbiddenBrowse(ref DataTable dataTable, ref int grants, ref int inheritMask, ArrayList arrayListGrants)
		{
			for(int i =0; i<arrayListGrants.Count; i++)
			{
				if (arrayListGrants[i] == null || !(arrayListGrants[i] is GrantsRow))
					continue;

				GrantsRow rowGrants	= (GrantsRow)arrayListGrants[i];
				DataRow dr	= dataTable.Rows[i];
				GrantsRow.IconState iconState = (GrantsRow.IconState)dr[securityGrants.Assign];

				if (Convert.ToInt32(dataTable.Rows[i][securityGrants.GrantMask])== 1024 || 
					Convert.ToInt32(dataTable.Rows[i][securityGrants.GrantMask])== 64) 
				{
					dataTable.Rows[i].BeginEdit();
					dataTable.Rows[i][securityGrants.Assign] = GrantsRow.IconState.IconForbidden;
					grants      = Bit.SetZero(grants, rowGrants.Mask);
					inheritMask = Bit.SetZero(inheritMask, rowGrants.Mask);
					dataTable.Rows[i].EndEdit();
				}
				
			}
		}
		#endregion

		public static void SetValueForAllGrants(GrantsDataGrid dataGrid, GrantOperationType grantValue)
		{
			DataTable dt = dataGrid.DataSource.Tables["Grants"];
			if (dt == null)
				return;
		
			foreach(DataRow dr in dt.Rows)
			{
				dr.BeginEdit();	
				dr[securityGrants.Assign] = (int)grantValue;
				dr.AcceptChanges();
			}

		}

	}
	//=========================================================================
}
