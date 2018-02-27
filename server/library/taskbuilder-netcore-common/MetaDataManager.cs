using Microarea.Common.NameSolver;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using TaskBuilderNetCore.Interfaces;

namespace Microarea.Common
{
    //-------------------------------------------------------------------------
    public class MetaDataManagerTool
    {
        private string connectionStringStandard;
        private string connectionStringCustom;
        private PathFinder pf;
        private string instanceKey;
        private SqlConnection connect = null;
        private SqlCommand mySQLCommand = null;


        //---------------------------------------------------------------------
        public MetaDataManagerTool(string instanceKey, string aConnectionStringStandardstring, string serverName, string installation)
        {
            this.instanceKey = instanceKey;
            //pf = new PathFinder("USR-BAUZONEANN", "DEVELOPMENT_NEXT", "ERP_NEWDB", "sa");
            //pf = new PathFinder("USR-calandrini", "DEv_next", "WebMago", "sa");
            pf = new PathFinder(serverName, installation, "pippo", "sa");
           
            // pf = new PathFinder(":microarea.database.windows.net", "DEV_ERP_NEXT", "WebMago", "sa");
            pf.Edition = "Professional";
            connectionStringStandard = aConnectionStringStandardstring;
            //  connectionStringStandard = "Data Source =USR-calandrini; Initial Catalog = 'Sysdb4'; User ID = 'sa'; Password = 'Microarea.'; Connect Timeout = 30; Pooling = false; ";
            // connectionStringStandard = "Server=tcp:microarea.database.windows.net;Database='ProvisioningDB';User ID='AdminMicroarea';Password='S1cr04$34!';Connect Timeout=30;";
            //connectionStringStandard = "Data Source = USR-BAUZONEANN; Initial Catalog = 'SystemDB_NewDB'; User ID = 'sa'; Password = '16'; Connect Timeout = 30; ";

            // connectionStringStandard = "Data Source=microarea.database.windows.net;Initial Catalog='ProvisioningDB';User ID='AdminMicroarea';Password='S1cr04$34!';Connect Timeout=30;Pooling=false;";
            // connectionStringStandard = "Data Source=microarea.database.windows.net;Initial Catalog='ProvisioningDB';User ID='AdminMicroarea';Password='S1cr04$34!';Connect Timeout=30;Pooling=false;";
            connectionStringCustom = ""; ;
        }

        //---------------------------------------------------------------------
        public MetaDataManagerTool(string aConnectionStringStandard, string aConnectionStringCustom)
        {
            connectionStringStandard = aConnectionStringStandard;
            connectionStringCustom = aConnectionStringCustom;

            pf = new PathFinder("USR-grillolara1", "DEvelopment", "company", "sa");
            pf.Edition = "Professional";
        }

        //---------------------------------------------------------------------
        public void InsertAllStandardMetaDataInDB()
        {
            connect = new SqlConnection(connectionStringStandard);
            connect.Open();

            string sInsert = @"insert into  MP_InstanceTBFS (InstanceKey, ParentID, Namespace, Application, Module, PathName, CompleteFileName, FileName, FileType, FileSize, ObjectType, CreationTime, LastWriteTime, IsDirectory, IsReadOnly, FileContent, FileTextContent)
					output INSERTED.FileID
                    VALUES
					(@InstanceKey, @ParentID, @Namespace, @Application, @Module,  @PathName, @CompleteFileName,  @FileName, @FileType, @FileSize, @ObjectType, @CreationTime, @LastWriteTime, @IsDirectory, @IsReadOnly, @FileContent, @FileTextContent)";

            mySQLCommand = new SqlCommand(sInsert, connect);
            mySQLCommand.Parameters.AddWithValue("@InstanceKey", instanceKey);
            mySQLCommand.Parameters.Add(new SqlParameter("@ParentID", SqlDbType.Int));
            mySQLCommand.Parameters.Add(new SqlParameter("@Namespace", SqlDbType.VarChar));
            mySQLCommand.Parameters.Add(new SqlParameter("@PathName", SqlDbType.VarChar));
            mySQLCommand.Parameters.Add(new SqlParameter("@Application", SqlDbType.VarChar));
            mySQLCommand.Parameters.Add(new SqlParameter("@Module", SqlDbType.VarChar));
            mySQLCommand.Parameters.Add(new SqlParameter("@CompleteFileName", SqlDbType.VarChar));
            mySQLCommand.Parameters.Add(new SqlParameter("@FileName", SqlDbType.VarChar));
            mySQLCommand.Parameters.Add(new SqlParameter("@FileType", SqlDbType.VarChar));
            mySQLCommand.Parameters.Add(new SqlParameter("@FileSize", SqlDbType.Int));
            mySQLCommand.Parameters.Add(new SqlParameter("@ObjectType", SqlDbType.VarChar));
            mySQLCommand.Parameters.Add(new SqlParameter("@CreationTime", SqlDbType.DateTime));
            mySQLCommand.Parameters.Add(new SqlParameter("@LastWriteTime", SqlDbType.DateTime));
            mySQLCommand.Parameters.Add(new SqlParameter("@IsDirectory", SqlDbType.Char));
            mySQLCommand.Parameters.Add(new SqlParameter("@IsReadOnly", SqlDbType.Char));
            mySQLCommand.Parameters.Add(new SqlParameter("@FileTextContent", SqlDbType.VarChar));

            SqlParameter contentParam = new SqlParameter("@FileContent", SqlDbType.VarBinary);
            mySQLCommand.Parameters.Add(contentParam);


            foreach (ApplicationInfo ai in pf.ApplicationInfos)
            {

                if (ai.ApplicationType != ApplicationType.TaskBuilderApplication &&
                        ai.ApplicationType != ApplicationType.TaskBuilder )
                    continue;

                if (ai.Modules == null || ai.Modules.Count == 0)
                    continue;
                int id;
                bool isCustom = ai.Path.Contains("Custom");
                DirectoryInfo dir = new DirectoryInfo(ai.Path);
                //inserisco nel db la directory dell app
                id = InsertDir(dir, isCustom, 0, ai.Name, string.Empty);
                //...quindi, per ogni modulo...
                if (File.Exists(Path.Combine(dir.FullName, NameSolverStrings.Application + NameSolverStrings.ConfigExtension)))
                    InsertFile(new FileInfo(dir.FullName + NameSolverStrings.Directoryseparetor + NameSolverStrings.Application + NameSolverStrings.ConfigExtension), isCustom, id, ai.Name, string.Empty);

                //File di brand
                OneLevelInsertFileByType(dir.FullName + NameSolverStrings.Directoryseparetor + "Solutions", id, isCustom, ai.Name, string.Empty, ".Brand.xml");
                //File di temi
                ThemesInsert(dir.FullName + NameSolverStrings.Directoryseparetor + "Themes", id, isCustom, ai.Name, string.Empty);

                foreach (ModuleInfo mi in ai.Modules)
                {
                    dir = new DirectoryInfo(mi.Path);
                    int idModulo = InsertDir(dir, isCustom, id, ai.Name, mi.Name);

                    if (File.Exists(Path.Combine(dir.FullName, NameSolverStrings.Module + NameSolverStrings.ConfigExtension)))
                        InsertFile(new FileInfo(dir.FullName + NameSolverStrings.Directoryseparetor + NameSolverStrings.Module + NameSolverStrings.ConfigExtension), isCustom, idModulo, ai.Name, mi.Name);

                    if (Directory.Exists(mi.Path + NameSolverStrings.Directoryseparetor + "ModuleObjects"))
                    {
                        dir = new DirectoryInfo(mi.Path + NameSolverStrings.Directoryseparetor + "ModuleObjects");
                        id = InsertDir(dir, isCustom, idModulo, ai.Name, mi.Name);
                        foreach (FileInfo file in dir.GetFiles())
                            InsertFile(file, isCustom, id, ai.Name, mi.Name); ;
                        foreach (DirectoryInfo subDir in dir.GetDirectories())//es ERP\CustomersSuppliers\ModuleObjects\CircularLetterTemplates
                        {
                            id = InsertDir(subDir, isCustom, id, ai.Name, mi.Name);
                            foreach (DirectoryInfo subDir2 in subDir.GetDirectories())//es ERP\CustomersSuppliers\ModuleObjects\CircularLetterTemplates\Description
                            {
                                id = InsertDir(subDir2, isCustom, id, ai.Name, mi.Name);
                                foreach (FileInfo file in subDir2.GetFiles())
                                    InsertFile(file, isCustom, id, ai.Name, mi.Name);
                                if (subDir2.FullName.Contains("ExportProfiles"))
                                {
                                    foreach (DirectoryInfo subDir3 in subDir2.GetDirectories())//es ERP\CustomersSuppliers\ModuleObjects\CircularLetterTemplates\Description
                                    {
                                        id = InsertDir(subDir3, isCustom, id, ai.Name, mi.Name);
                                        foreach (FileInfo file in subDir3.GetFiles())
                                            InsertFile(file, isCustom, id, ai.Name, mi.Name);
                                    }
                                }
                            }
                        }
                    }

                    OneLevelInsert(mi.Path + NameSolverStrings.Directoryseparetor + "Settings", idModulo, isCustom, ai.Name, mi.Name);

                    RecourseInsert(mi.Path + NameSolverStrings.Directoryseparetor + "Files", idModulo, isCustom, ai.Name, mi.Name);

                    OneLevelInsert(mi.Path + NameSolverStrings.Directoryseparetor + "ReferenceObjects", idModulo, isCustom, ai.Name, mi.Name);

                    OneLevelInsert(mi.Path + NameSolverStrings.Directoryseparetor + "Report", idModulo, isCustom, ai.Name, mi.Name);

                    OneLevelInsert(mi.Path + NameSolverStrings.Directoryseparetor + "Menu", idModulo, isCustom, ai.Name, mi.Name);

                    RecourseInsert(mi.Path + NameSolverStrings.Directoryseparetor + "JsonForms", idModulo, isCustom, ai.Name, mi.Name);

                    OneLevelInsert(mi.Path + NameSolverStrings.Directoryseparetor + "Companies", idModulo, isCustom, ai.Name, mi.Name);

                    RecourseInsert(mi.Path + NameSolverStrings.Directoryseparetor + "DataManager", idModulo, isCustom, ai.Name, mi.Name);

                    RecourseInsert(mi.Path + NameSolverStrings.Directoryseparetor + "DatabaseScript", idModulo, isCustom, ai.Name, mi.Name);

                    RecourseInsert(mi.Path + NameSolverStrings.Directoryseparetor + "XML", idModulo, isCustom, ai.Name, mi.Name);
                }

            }
              connect.Close();
        }

        //---------------------------------------------------------------------
        public void DeleteAllStandardMetaDataInFS()
        {
            foreach (ApplicationInfo ai in pf.ApplicationInfos)
            {
                if (ai.ApplicationType != ApplicationType.TaskBuilderApplication &&
                        ai.ApplicationType != ApplicationType.TaskBuilder)
                    continue;

                if (ai.Modules == null || ai.Modules.Count == 0)
                    continue;

                bool isCustom = ai.Path.Contains("Custom");
                DirectoryInfo dir = new DirectoryInfo(ai.Path);
                //inserisco nel db la directory dell app
                // DeleteDir(dir);
                //...quindi, per ogni modulo...
                if (File.Exists(Path.Combine(dir.FullName, NameSolverStrings.Application + NameSolverStrings.ConfigExtension)))
                    DeleteFile(new FileInfo(dir.FullName + NameSolverStrings.Directoryseparetor + NameSolverStrings.Application + NameSolverStrings.ConfigExtension));

                //File di brand
                OneLevelDeleteFileByType(dir.FullName + NameSolverStrings.Directoryseparetor + "Solutions", isCustom, ai.Name, string.Empty, ".Brand.xml");
                //File di temi
                ThemesDelete(dir.FullName + NameSolverStrings.Directoryseparetor + "Themes", isCustom, ai.Name, string.Empty);

                foreach (ModuleInfo mi in ai.Modules)
                {
                    dir = new DirectoryInfo(mi.Path);
                  //  int idModulo = InsertDir(dir, isCustom, id, ai.Name, mi.Name);

                    if (File.Exists(Path.Combine(dir.FullName, NameSolverStrings.Module + NameSolverStrings.ConfigExtension)))
                        DeleteFile(new FileInfo(dir.FullName + NameSolverStrings.Directoryseparetor + NameSolverStrings.Module + NameSolverStrings.ConfigExtension));

                    if (Directory.Exists(mi.Path + NameSolverStrings.Directoryseparetor + "ModuleObjects"))
                    {
                        dir = new DirectoryInfo(mi.Path + NameSolverStrings.Directoryseparetor + "ModuleObjects");
                     //   id = DeleteDir(dir, isCustom, idModulo, ai.Name, mi.Name);
                        foreach (FileInfo file in dir.GetFiles())
                            DeleteFile(file);
                        foreach (DirectoryInfo subDir in dir.GetDirectories())//es ERP\CustomersSuppliers\ModuleObjects\CircularLetterTemplates
                        {
                          //  id = InsertDir(subDir, isCustom, id, ai.Name, mi.Name);
                            foreach (DirectoryInfo subDir2 in subDir.GetDirectories())//es ERP\CustomersSuppliers\ModuleObjects\CircularLetterTemplates\Description
                            {
                            //    id = InsertDir(subDir2, isCustom, id, ai.Name, mi.Name);
                                foreach (FileInfo file in subDir2.GetFiles())
                                    DeleteFile(file);
                                if (subDir2.FullName.Contains("ExportProfiles"))
                                {
                                    foreach (DirectoryInfo subDir3 in subDir2.GetDirectories())//es ERP\CustomersSuppliers\ModuleObjects\CircularLetterTemplates\Description
                                    {
                             //         id = InsertDir(subDir3, isCustom, id, ai.Name, mi.Name);
                                        foreach (FileInfo file in subDir3.GetFiles())
                                            DeleteFile(file);
                                    }
                                }
                            }
                        }
                    }

                    OneLevelDelete(mi.Path + NameSolverStrings.Directoryseparetor + "Settings");

                    RecourseDelete(mi.Path + NameSolverStrings.Directoryseparetor + "Files");

                    OneLevelDelete(mi.Path + NameSolverStrings.Directoryseparetor + "ReferenceObjects");

                    OneLevelDelete(mi.Path + NameSolverStrings.Directoryseparetor + "Report");

                    OneLevelDelete(mi.Path + NameSolverStrings.Directoryseparetor + "Menu");

                    RecourseDelete(mi.Path + NameSolverStrings.Directoryseparetor + "JsonForms");

                    OneLevelDelete(mi.Path + NameSolverStrings.Directoryseparetor + "Companies");

                    RecourseDelete(mi.Path + NameSolverStrings.Directoryseparetor + "DataManager");

                    RecourseDelete(mi.Path + NameSolverStrings.Directoryseparetor + "DatabaseScript");

                    RecourseDelete(mi.Path + NameSolverStrings.Directoryseparetor + "XML");
                }

            }
            connect.Close();
        }

        //---------------------------------------------------------------------
        public void DeleteAllStandardMetaDataInDBByInstance(string instanceKey)
        {
            connect = new SqlConnection(connectionStringStandard);
            connect.Open();

            string sDelete = @"delete from  MP_InstanceTBFS where InstanceKey = " + instanceKey;
            mySQLCommand = new SqlCommand(sDelete, connect);
            mySQLCommand.ExecuteNonQuery();
            connect.Close();
        }

        //---------------------------------------------------------------------
        private void ThemesInsert(string folderPath, int idModulo, bool isCustom, string appName, string moduleName)
        {
            if (Directory.Exists(folderPath))
            {
                DirectoryInfo dir = new DirectoryInfo(folderPath);
                int id = InsertDir(dir, isCustom, idModulo, appName, moduleName);
                foreach (FileInfo file in dir.GetFiles())
                    InsertFile(file, isCustom, id, appName, moduleName);
                if (Directory.Exists(folderPath + NameSolverStrings.Directoryseparetor + "Images"))
                {
                    DirectoryInfo dirSub = new DirectoryInfo(folderPath + NameSolverStrings.Directoryseparetor + "Images");
                    int idsub = InsertDir(dirSub, isCustom, idModulo, appName, moduleName);
                    foreach (FileInfo subFile in dirSub.GetFiles())
                        InsertFile(subFile, isCustom, idsub, appName, moduleName);
                }

            }

        }
        //---------------------------------------------------------------------
        private void ThemesDelete(string folderPath, bool isCustom, string appName, string moduleName)
        {
            if (Directory.Exists(folderPath))
            {
                DirectoryInfo dir = new DirectoryInfo(folderPath);
               // int id = InsertDir(dir, isCustom, idModulo, appName, moduleName);
                foreach (FileInfo file in dir.GetFiles())
                    DeleteFile(file);
                if (Directory.Exists(folderPath + NameSolverStrings.Directoryseparetor + "Images"))
                {
                    DirectoryInfo dirSub = new DirectoryInfo(folderPath + NameSolverStrings.Directoryseparetor + "Images");
                //    int idsub = InsertDir(dirSub, isCustom, idModulo, appName, moduleName);
                    foreach (FileInfo subFile in dirSub.GetFiles())
                        DeleteFile(subFile);
                }

            }

        }
        //---------------------------------------------------------------------
        private void OneLevelInsertFileByType(string folderPath, int idModulo, bool isCustom, string appName, string moduleName, string fileExtension)
        {
            if (Directory.Exists(folderPath))
            {
                DirectoryInfo dir = new DirectoryInfo(folderPath);
                int id = InsertDir(dir, isCustom, idModulo, appName, moduleName);
                foreach (FileInfo file in dir.GetFiles())
                    if (file.Name.Contains(fileExtension))
                        InsertFile(file, isCustom, id, appName, moduleName);
            }

        }

        //---------------------------------------------------------------------
        private void OneLevelDeleteFileByType(string folderPath,  bool isCustom, string appName, string moduleName, string fileExtension)
        {
            if (Directory.Exists(folderPath))
            {
                DirectoryInfo dir = new DirectoryInfo(folderPath);
            //    int id = InsertDir(dir, isCustom, idModulo, appName, moduleName);
                foreach (FileInfo file in dir.GetFiles())
                    if (file.Name.Contains(fileExtension))
                        DeleteFile(file);
            }

        }

        //---------------------------------------------------------------------
        private void RecourseInsert(string folderPath, int idModulo, bool isCustom, string appName, string moduleName)
        {
            if (Directory.Exists(folderPath))
            {
                DirectoryInfo dir = new DirectoryInfo(folderPath);
                int id = InsertDir(dir, isCustom, idModulo, appName, moduleName);
                foreach (FileInfo file in dir.GetFiles())
                    InsertFile(file, isCustom, id, appName, moduleName);

                foreach (DirectoryInfo info in dir.GetDirectories())
                    RecourseInsert(info.FullName, idModulo, isCustom, appName, moduleName);
            }

        }


        //---------------------------------------------------------------------
        private void RecourseDelete(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                DirectoryInfo dir = new DirectoryInfo(folderPath);
             //   int id = InsertDir(dir, isCustom, idModulo, appName, moduleName);
                foreach (FileInfo file in dir.GetFiles())
                    DeleteFile(file);

                foreach (DirectoryInfo info in dir.GetDirectories())
                    RecourseDelete(info.FullName);
            }

        }

        //---------------------------------------------------------------------
        private void OneLevelInsert(string folderPath, int idModulo, bool isCustom, string appName, string moduleName)
        {
            if (Directory.Exists(folderPath))
            {
                DirectoryInfo dir = new DirectoryInfo(folderPath);
                int id = InsertDir(dir, isCustom, idModulo, appName, moduleName);
                foreach (FileInfo file in dir.GetFiles())
                    InsertFile(file, isCustom, id, appName, moduleName);
            }

        }

        //---------------------------------------------------------------------
        private void OneLevelDelete(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                DirectoryInfo dir = new DirectoryInfo(folderPath);
              //  int id = InsertDir(dir, isCustom, idModulo, appName, moduleName);
                foreach (FileInfo file in dir.GetFiles())
                    DeleteFile(file);
            }

        }


        //---------------------------------------------------------------------
        private void DeleteDir(DirectoryInfo aDir)
        {
            aDir.Delete();
        }
        //---------------------------------------------------------------------
        private int InsertDir(DirectoryInfo aDir, bool isCustom, int id, string appName, string moduleName)
        {
    
            int index = aDir.FullName.IndexOf(@"Standard\", 0);
            string path = aDir.FullName.Substring(index + 9);
            string folderNameSpace = path.Substring(12);
            folderNameSpace = "Folder." + folderNameSpace.Replace('\\', '.');
            try
            {

                if (id == 0)
                    mySQLCommand.Parameters["@ParentID"].Value = DBNull.Value;
                else
                    mySQLCommand.Parameters["@ParentID"].Value = id;

                mySQLCommand.Parameters["@Namespace"].Value = folderNameSpace;
                mySQLCommand.Parameters["@PathName"].Value = path;
                mySQLCommand.Parameters["@Application"].Value = appName;
                mySQLCommand.Parameters["@Module"].Value = moduleName;
                mySQLCommand.Parameters["@CompleteFileName"].Value = path;
                mySQLCommand.Parameters["@FileName"].Value = String.Empty;
                mySQLCommand.Parameters["@FileType"].Value = String.Empty;
                mySQLCommand.Parameters["@FileSize"].Value = 0;
                mySQLCommand.Parameters["@ObjectType"].Value = "DIRECTORY";
                mySQLCommand.Parameters["@CreationTime"].Value = aDir.CreationTime;
                mySQLCommand.Parameters["@LastWriteTime"].Value = aDir.LastWriteTime;
                mySQLCommand.Parameters["@IsDirectory"].Value = "1";
                mySQLCommand.Parameters["@IsReadOnly"].Value = "0";
                mySQLCommand.Parameters["@FileTextContent"].Value = DBNull.Value;
                mySQLCommand.Parameters["@FileContent"].Value = DBNull.Value;

                return (int)mySQLCommand.ExecuteScalar();
            }

            catch (SqlException)
            {
                return 0;
            }
            finally
            {

            }
        }

        ////---------------------------------------------------------------------
        //private string GetType(string nameSpace, string fileFullName)
        //{
        //    string fileType = string.Empty;

        //    if (nameSpace != string.Empty)
        //        return nameSpace.Substring(0, nameSpace.IndexOf('.')).ToUpper();

        //    if (fileFullName.ToLower().Contains("application.config"))
        //        return "APPLICATION";

        //    if (fileFullName.ToLower().Contains("module.config"))
        //        return "MODULE";

        //    if (fileFullName.ToLower().Contains(".menu"))
        //        return "MENU";

        //    if (fileFullName.ToLower().Contains("description"))
        //        return "DESCRIPTION";

        //    if (fileFullName.ToLower().Contains("exportprofiles"))
        //        return "EXPORTPROFILES";

        //    if (fileFullName.ToLower().Contains("datamanager"))
        //        return "DATA";

        //    if (fileFullName.ToLower().Contains("brand"))
        //        return "BRAND";

        //    if (fileFullName.ToLower().Contains("themes"))
        //        return "THEMES";

        //    if (fileFullName.ToLower().Contains(".sql"))
        //        return "SQL";

        //    if (fileFullName.ToLower().Contains(".hjson") || fileFullName.ToLower().Contains(".tbjson"))
        //        return "JSONFORM";

        //    if (fileFullName.ToLower().Contains("settings.config") || fileFullName.ToLower().Contains("\\settings\\"))
        //        return "SETTING";

        //    if (fileFullName.ToLower().Contains(".gif") || fileFullName.ToLower().Contains(".jpg") || fileFullName.ToLower().Contains(".png"))
        //        return "IMAGE";

        //    if (fileFullName.ToLower().Contains("\\moduleobjects\\"))
        //    {
        //        string fileName = fileFullName.Substring(fileFullName.LastIndexOf('\\') + 1);
        //        fileName = fileName.Substring(0, fileName.LastIndexOf('.'));
        //        return fileName.ToUpper();
        //    }
        //    return string.Empty;
        //}


        //---------------------------------------------------------------------
        private void DeleteFile(FileInfo aFile)
        {
            aFile.Delete();
           
        }
        //---------------------------------------------------------------------
        private void InsertFile(FileInfo aFile, bool isCustom, int id, string appName, string moduleName)
        {
            try
            {
                string nameSpace = string.Empty;
				string fileFullName = aFile.FullName;

                if (PathFinder.PathFinderInstance.GetNamespaceFromPath(fileFullName) != null)
                {
                    nameSpace = PathFinder.PathFinderInstance.GetNamespaceFromPath(aFile.FullName).ToString();
                    if (nameSpace.Contains(".wrm"))
                        nameSpace = nameSpace.Substring(0, nameSpace.LastIndexOf('.'));
                }
                

				string fileType = string.Empty;
				bool isText = true;

                if (nameSpace != string.Empty)
                    fileType = nameSpace.Substring(0, nameSpace.IndexOf('.')).ToUpper();
                else if (fileFullName.ToLower().Contains("application.config"))
                    fileType = "APPLICATION";
                else if (fileFullName.ToLower().Contains("module.config"))
                    fileType = "MODULE";
                else if (fileFullName.ToLower().Contains(".menu"))
                    fileType = "MENU";
                else if (fileFullName.ToLower().Contains("description"))
                    fileType = "DESCRIPTION";
                else if (fileFullName.ToLower().Contains("exportprofiles"))
                    fileType = "EXPORTPROFILES";
                else if (fileFullName.ToLower().Contains("datamanager"))
                    fileType = "DATA";
                else if (fileFullName.ToLower().Contains("brand"))
                    fileType = "BRAND";
                //else if (fileFullName.ToLower().Contains("themes"))
                //    fileType = "THEMES";
                else if (fileFullName.ToLower().Contains(".sql"))
                    fileType = "SQL";
                else if (fileFullName.ToLower().Contains(".hjson") || fileFullName.ToLower().Contains(".tbjson"))
                    fileType = "JSONFORM";
                else if (fileFullName.ToLower().Contains("settings.config") || fileFullName.ToLower().Contains("\\settings\\"))
                    fileType = "SETTING";
                else if (fileFullName.ToLower().Contains("\\moduleobjects\\"))
                {
                    string fileName = fileFullName.Substring(fileFullName.LastIndexOf('\\') + 1);
                    fileName = fileName.Substring(0, fileName.LastIndexOf('.'));
                    fileType = fileName.ToUpper();
                }

                if (fileFullName.ToLower().Contains(".css")|| fileFullName.ToLower().Contains(".js") || fileFullName.ToLower().Contains(".map")||
                    fileFullName.ToLower().Contains(".png")||fileFullName.ToLower().Contains(".gif") || fileFullName.ToLower().Contains(".bmp")
                    || fileFullName.ToLower().Contains(".jpg") || fileFullName.ToLower().Contains(".eot") || fileFullName.ToLower().Contains(".svg")
                    || fileFullName.ToLower().Contains(".ttf") || fileFullName.ToLower().Contains(".woff") || fileFullName.ToLower().Contains(".html")
                    || fileFullName.ToLower().Contains(".scss") || fileFullName.ToLower().Contains(".htm") || fileFullName.ToLower().Contains(".jpeg")
                   )
                {
                    isText = false;

                }
                
                int index = aFile.FullName.IndexOf(@"Standard\", 0);

                mySQLCommand.Parameters["@ParentID"].Value =  id;
                mySQLCommand.Parameters["@Namespace"].Value = nameSpace;
                mySQLCommand.Parameters["@PathName"].Value = aFile.FullName.Substring(index + 9, aFile.FullName.LastIndexOf('\\') - (index + 9));
                mySQLCommand.Parameters["@FileName"].Value = aFile.Name;
                mySQLCommand.Parameters["@CompleteFileName"].Value = aFile.FullName.Substring(index + 9);
                mySQLCommand.Parameters["@FileType"].Value = aFile.Extension;
                mySQLCommand.Parameters["@Application"].Value = appName;
                mySQLCommand.Parameters["@Module"].Value = moduleName;
                mySQLCommand.Parameters["@FileSize"].Value = aFile.Length;
				mySQLCommand.Parameters["@ObjectType"].Value = fileType;
				mySQLCommand.Parameters["@CreationTime"].Value = aFile.CreationTime;
                mySQLCommand.Parameters["@LastWriteTime"].Value = aFile.LastWriteTime;
                mySQLCommand.Parameters["@IsDirectory"].Value = "0";
                mySQLCommand.Parameters["@IsReadOnly"].Value = aFile.IsReadOnly ? "1" : "0";

				byte[] byteContent = null;
                if (!isText) 
                {
						byteContent = FileToByteArray(aFile.FullName, aFile.Length);
						mySQLCommand.Parameters["@FileContent"].Value = byteContent;
						mySQLCommand.Parameters["@FileTextContent"].Value = DBNull.Value;						
                }
                else
                {
                        mySQLCommand.Parameters["@FileTextContent"].Value = FileToString(aFile.FullName);
                        mySQLCommand.Parameters["@FileContent"].Value = DBNull.Value;
                }
      
                mySQLCommand.ExecuteNonQuery();

				if (byteContent != null)
					byteContent = null;
			}

            catch (SqlException exx)
            {
            }
            finally
            {

            }
        }

        //---------------------------------------------------------------------
        public static bool isBinary(string path, long lenght)
        {
               if (lenght == 0) return false;

            using (StreamReader stream = new StreamReader(path))
            {
                int ch;
                while ((ch = stream.Read()) != -1)
                {
                    if ((ch > Chars.NUL && ch < Chars.BS) || (ch > Chars.CR && ch < Chars.SUB))
					{
                        return true;
                    }
                }
            }
            return false;
        }

        //---------------------------------------------------------------------
        public static class Chars
        {
            public static char NUL = (char)0; // Null char
            public static char BS = (char)8; // Back Space
            public static char CR = (char)13; // Carriage Return
            public static char SUB = (char)26; // Substitute
        }

        //---------------------------------------------------------------------
        public byte[] FileToByteArray(string fileName, long numBytes)
        {
            byte[] buff = null;
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            buff = br.ReadBytes((int)numBytes);
            return buff;
        }

        //---------------------------------------------------------------------
        public string FileToString(string fileName)
        {
            string text;
            StreamReader streamReader = new StreamReader(fileName);
            
            text = streamReader.ReadToEnd();

            return text;
        }

    }
}
