using Microarea.Common.NameSolver;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using TaskBuilderNetCore.Interfaces;

namespace Microarea.Common
{
    //-------------------------------------------------------------------------
    class MetaDataManagerTool
    {
        private string connectionStringStandard;
        private string connectionStringCustom;
        
        //---------------------------------------------------------------------
        public MetaDataManagerTool(string aConnectionStringStandard, string aConnectionStringCustom)
        {
            connectionStringStandard = aConnectionStringStandard;
            connectionStringCustom = aConnectionStringCustom;
        }
        //---------------------------------------------------------------------
        public void InsertAllMetaDataInDB()
        {
            //TODO LARA
         //   bool connect = databaseManager.ContextInfo.MakeCompanyConnection(databaseManager.ContextInfo.CompanyId);

            foreach (BaseApplicationInfo ai in BasePathFinder.BasePathFinderInstance.ApplicationInfos)
            {

                if (ai.ApplicationType != ApplicationType.TaskBuilderApplication &&
                        ai.ApplicationType != ApplicationType.TaskBuilder &&
                        ai.ApplicationType != ApplicationType.Customization)
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
                    InsertFile(new FileInfo(dir.FullName + Path.DirectorySeparatorChar + NameSolverStrings.Application + NameSolverStrings.ConfigExtension), isCustom, id, ai.Name, string.Empty);


                foreach (BaseModuleInfo mi in ai.Modules)
                {
                    dir = new DirectoryInfo(mi.Path);
                    int idModulo = InsertDir(dir, isCustom, id, ai.Name, mi.Name);

                    if (File.Exists(Path.Combine(dir.FullName, NameSolverStrings.Module + NameSolverStrings.ConfigExtension)))
                        InsertFile(new FileInfo(dir.FullName + Path.DirectorySeparatorChar + NameSolverStrings.Module + NameSolverStrings.ConfigExtension), isCustom, idModulo, ai.Name, mi.Name);

                    if (Directory.Exists(mi.Path + Path.DirectorySeparatorChar + "ModuleObjects"))
                    {
                        dir = new DirectoryInfo(mi.Path + Path.DirectorySeparatorChar + "ModuleObjects");
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
                                    foreach (DirectoryInfo subDir3 in subDir.GetDirectories())//es ERP\CustomersSuppliers\ModuleObjects\CircularLetterTemplates\Description
                                    {
                                        id = InsertDir(subDir3, isCustom, id, ai.Name, mi.Name);
                                        foreach (FileInfo file in subDir3.GetFiles())
                                            InsertFile(file, isCustom, id, ai.Name, mi.Name);
                                    }
                                }
                            }
                        }
                    }

                    OneLevelInsert(mi.Path + Path.DirectorySeparatorChar + "Settings", idModulo, isCustom, ai.Name, mi.Name);

                    OneLevelInsert(mi.Path + Path.DirectorySeparatorChar + "Files", idModulo, isCustom, ai.Name, mi.Name);

                    OneLevelInsert(mi.Path + Path.DirectorySeparatorChar + "ReferenceObjects", idModulo, isCustom, ai.Name, mi.Name);

                    OneLevelInsert(mi.Path + Path.DirectorySeparatorChar + "Report", idModulo, isCustom, ai.Name, mi.Name);

                    OneLevelInsert(mi.Path + Path.DirectorySeparatorChar + "Menu", idModulo, isCustom, ai.Name, mi.Name);

                    OneLevelInsert(mi.Path + Path.DirectorySeparatorChar + "JsonForms", idModulo, isCustom, ai.Name, mi.Name);

                    OneLevelInsert(mi.Path + Path.DirectorySeparatorChar + "Companies", idModulo, isCustom, ai.Name, mi.Name);

                    RecourseInsert(mi.Path + Path.DirectorySeparatorChar + "DataManager", idModulo, isCustom, ai.Name, mi.Name);

                    RecourseInsert(mi.Path + Path.DirectorySeparatorChar + "DatabaseScript", idModulo, isCustom, ai.Name, mi.Name);
                }

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
        private int InsertDir(DirectoryInfo aDir, bool isCustom, int id, string appName, string moduleName)
        {
            //if (databaseManager.ContextInfo.Connection == null || databaseManager.ContextInfo.Connection.State != System.Data.ConnectionState.Open)
            //    return -1;

            SqlCommand mySQLCommand = null;

            string tableName = "TB_StandardMetadata";

            if (isCustom)
                tableName = "TB_CustomMetadata";

            string sInsert = @"INSERT INTO " + tableName + @" (ParentID, Namespace, Application, Module, PathName, Name, FileType, FileSize, ObjectType, CreationTime, LastWriteTime, IsDirectory, IsReadOnly, FileContent)
					output INSERTED.FileID
                    VALUES
					(@ParentID, @Namespace, @Application, @Module,  @PathName, @Name, @FileType, @FileSize, @ObjectType, @CreationTime, @LastWriteTime, @IsDirectory, @IsReadOnly, @FileContent)";


            if (isCustom)
            {
                mySQLCommand = new SqlCommand(connectionStringCustom);
                tableName = "TB_CustomMetadata";
            }
            else
            {
                SqlConnection connect = new SqlConnection(connectionStringStandard);
                connect.Open();
                mySQLCommand = new SqlCommand(sInsert, connect);
            }

            int index = aDir.FullName.IndexOf(@"Standard\", 0);
            string path = aDir.FullName.Substring(index + 9);
            string folderNameSpace = path.Substring(12);
            folderNameSpace = "Folder." + folderNameSpace.Replace('\\', '.');
            try
            {

                if (id == 0)
                    mySQLCommand.Parameters.AddWithValue("@ParentID", DBNull.Value);
                else
                    mySQLCommand.Parameters.AddWithValue("@ParentID", id);

                mySQLCommand.Parameters.AddWithValue("@Namespace", folderNameSpace);// aDir.Name);
                mySQLCommand.Parameters.AddWithValue("@PathName", path);
                mySQLCommand.Parameters.AddWithValue("@Application", appName);
                mySQLCommand.Parameters.AddWithValue("@Module", moduleName);
                mySQLCommand.Parameters.AddWithValue("@Name", String.Empty);
                mySQLCommand.Parameters.AddWithValue("@FileType", String.Empty);
                mySQLCommand.Parameters.AddWithValue("@FileSize", 0);
                mySQLCommand.Parameters.AddWithValue("@ObjectType", "DIRECTORY");
                mySQLCommand.Parameters.AddWithValue("@CreationTime", aDir.CreationTime);
                mySQLCommand.Parameters.AddWithValue("@LastWriteTime", aDir.LastWriteTime);
                mySQLCommand.Parameters.AddWithValue("@IsDirectory", "1");
                mySQLCommand.Parameters.AddWithValue("@IsReadOnly", "0");

                SqlParameter contentParam = new SqlParameter("@FileContent", System.Data.SqlDbType.VarBinary);
                contentParam.Value = DBNull.Value;
                mySQLCommand.Parameters.Add(contentParam);

                int modified = (int)mySQLCommand.ExecuteScalar();

                mySQLCommand.Dispose();

                return modified;

            }

            catch (SqlException)
            {
                return 0;
            }
            finally
            {
                if (mySQLCommand != null)
                    mySQLCommand.Dispose();
            }

        }

        //---------------------------------------------------------------------
        private string GetType(string nameSpace, string fileFullName)
        {
            string fileType = string.Empty;

            if (nameSpace != string.Empty)
                return nameSpace.Substring(0, nameSpace.IndexOf('.')).ToUpper();

            if (fileFullName.ToLower().Contains("application.config"))
                return "APPLICATION";

            if (fileFullName.ToLower().Contains("module.config"))
                return "MODULE";

            if (fileFullName.ToLower().Contains(".menu"))
                return "MENU";

            if (fileFullName.ToLower().Contains("description"))
                return "DESCRIPTION";

            if (fileFullName.ToLower().Contains("exportprofiles"))
                return "EXPORTPROFILES";

            if (fileFullName.ToLower().Contains("datamanager"))
                return "DATA";

            if (fileFullName.ToLower().Contains(".sql"))
                return "SQL";

            if (fileFullName.ToLower().Contains(".hjson") || fileFullName.ToLower().Contains(".tbjson"))
                return "JSONFORM";

            if (fileFullName.ToLower().Contains("settings.config") || fileFullName.ToLower().Contains("\\settings\\"))
                return "SETTING";

            if (fileFullName.ToLower().Contains(".gif") || fileFullName.ToLower().Contains(".jpg") || fileFullName.ToLower().Contains(".png"))
                return "IMAGE";

            if (fileFullName.ToLower().Contains("\\moduleobjects\\"))
            {
                string fileName = fileFullName.Substring(fileFullName.LastIndexOf('\\') + 1);
                fileName = fileName.Substring(0, fileName.LastIndexOf('.'));
                return fileName.ToUpper();
            }
            return string.Empty;
        }
        //---------------------------------------------------------------------
        private void InsertFile(FileInfo aFile, bool isCustom, int id, string appName, string moduleName)
        {
            //if (databaseManager.ContextInfo.Connection == null || databaseManager.ContextInfo.Connection.State != System.Data.ConnectionState.Open)
            //    return;

            SqlCommand mySQLCommand = null;

            string tableName = "TB_StandardMetadata";

            if (isCustom)
                tableName = "TB_CustomMetadata";

            //Inserisco i Ruoli
            string sInsert = @"INSERT INTO " + tableName + @" (ParentID, Namespace, Application, Module, PathName, Name, FileType, FileSize, ObjectType, CreationTime, LastWriteTime, IsDirectory, IsReadOnly, FileContent)
					VALUES
					(@ParentID, @Namespace, @Application, @Module, @PathName, @Name, @FileType, @FileSize, @ObjectType, @CreationTime, @LastWriteTime, @IsDirectory, @IsReadOnly, @FileContent)";



            if (isCustom)
            {
                mySQLCommand = new SqlCommand(connectionStringCustom);
                tableName = "TB_CustomMetadata";
            }
            else
            {
                SqlConnection connect = new SqlConnection(connectionStringStandard);
                connect.Open();
                mySQLCommand = new SqlCommand(sInsert, connect);
            }


            //try
            //{

            string nameSpace = string.Empty;
            if (BasePathFinder.BasePathFinderInstance.GetNamespaceFromPath(aFile.FullName) != null)
            {
                nameSpace = BasePathFinder.BasePathFinderInstance.GetNamespaceFromPath(aFile.FullName).ToString();
                if (nameSpace.Contains(".wrm"))
                    nameSpace = nameSpace.Substring(0, nameSpace.LastIndexOf('.'));
            }


            int index = aFile.FullName.IndexOf(@"Standard\", 0);

            mySQLCommand.Parameters.AddWithValue("@ParentID", id);
            mySQLCommand.Parameters.AddWithValue("@Namespace", nameSpace);
            mySQLCommand.Parameters.AddWithValue("@PathName", aFile.FullName.Substring(index + 9, aFile.FullName.LastIndexOf('\\') - (index + 9)));
            mySQLCommand.Parameters.AddWithValue("@Name", aFile.Name);
            mySQLCommand.Parameters.AddWithValue("@FileType", aFile.Extension.Substring(1));
            mySQLCommand.Parameters.AddWithValue("@Application", appName);
            mySQLCommand.Parameters.AddWithValue("@Module", moduleName);
            mySQLCommand.Parameters.AddWithValue("@FileSize", aFile.Length);
            mySQLCommand.Parameters.AddWithValue("@ObjectType", GetType(nameSpace, aFile.FullName));
            mySQLCommand.Parameters.AddWithValue("@CreationTime", aFile.CreationTime);
            mySQLCommand.Parameters.AddWithValue("@LastWriteTime", aFile.LastWriteTime);
            mySQLCommand.Parameters.AddWithValue("@IsDirectory", "0");
            mySQLCommand.Parameters.AddWithValue("@IsReadOnly", aFile.IsReadOnly ? "1" : "0");

            SqlParameter contentParam = new SqlParameter("@FileContent", System.Data.SqlDbType.VarBinary);
            contentParam.Value = FileToByteArray(aFile.FullName);
            mySQLCommand.Parameters.Add(contentParam);


            //   mySQLCommand.Parameters.Add("@FileContent", String.Empty);
            mySQLCommand.ExecuteNonQuery();

            mySQLCommand.Dispose();



            //}

            //catch (SqlException)
            //{
            //}
            //finally
            //{
            //    if (mySQLCommand != null)
            //        mySQLCommand.Dispose();
            //}

        }

        //---------------------------------------------------------------------
        public byte[] FileToByteArray(string fileName)
        {
            byte[] buff = null;
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            long numBytes = new FileInfo(fileName).Length;
            buff = br.ReadBytes((int)numBytes);
            return buff;
        }
    }
}
