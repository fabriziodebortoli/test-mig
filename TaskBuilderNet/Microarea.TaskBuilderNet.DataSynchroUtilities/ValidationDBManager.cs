using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.DataSynchroUtilities.Validation;
using Microarea.TaskBuilderNet.DataSynchroUtilities.Validation.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.TaskBuilderNet.DataSynchroUtilities
{
    public interface IValidationDBManager
    {
        int DeleteValidationInfo(TBConnection myConnection, string providerName);
        int DeleteValidationInfo(TBConnection myConnection, string providerName, string docTBGuid);
        int UpdateValidationInfo(TBConnection myConnection, string docTBGuid, string actionName, string docNamespace, DateTime validationDate, IAFError dataError, int workerID, string providerName, string whereProviderName);
        int InsertValidationInfo(TBConnection myConnection, string docTBGuid, string actionName, string docNamespace, DateTime validationDate, IAFError dataError, int workerID, bool bUsedForFilter, string providerName, string whereProviderName);
        bool ExistRecordInValidationInfo(TBConnection myConnection, string docTBGuid, string providerName, string whereProviderName);
        bool ExistRecordInValidationFKtoFix(TBConnection myConnection, string providerName, string docNamespace, string qualifiedField, string value, string whereProviderName);
        void DeleteFKToFixRecordsForMassiveValidation(TBConnection myConnection, string providerName);
        void InsertValidationFKtoFix(TBConnection myConnection, string providerName, string docNamespace, string qualifiedField, string value, DateTime validationDate, int workerID, string whereProviderName);
        void UpdateValidationFKtoFixWithRelatedErrors(TBConnection myConnection, string providerName, string whereProviderName);

        void Flush(string connectionString);
    }

    public class SimpleValidationDBManager : IValidationDBManager
    {
        private static object _locker = "";
        private static SimpleValidationDBManager _instance = null;
        public static SimpleValidationDBManager GetInstance(int maxCapacityCache)
        {
            lock (_locker)
            {
                if (_instance == null)
                {
                    _instance = new SimpleValidationDBManager(maxCapacityCache);
                }
                return _instance;
            }
        }
        public void Flush(string connectionString)
        {
            lock (_locker)
            {
                DoBulkInsert(connectionString);
            }

        }
        private static DataTable MakeDS_ValidationInfoDataTable()
        {
            try
            {
                DataTable table = new DataTable("DS_ValidationInfo");

                // Add columns
                table.Columns.Add("ProviderName", typeof(String));
                table.Columns.Add("DocTBGuid", typeof(Guid));
                table.Columns.Add("ActionName", typeof(String));
                table.Columns.Add("DocNamespace", typeof(String));
                table.Columns.Add("FKError", typeof(String));
                table.Columns.Add("XSDError", typeof(String));
                table.Columns.Add("UsedForFilter", typeof(String));
                table.Columns.Add("MessageError", typeof(String));
                table.Columns.Add("ValidationDate", typeof(DateTime));
                table.Columns.Add("TBCreated", typeof(DateTime));
                table.Columns.Add("TBModified", typeof(DateTime));
                table.Columns.Add("TBCreatedID", typeof(Int32));
                table.Columns.Add("TBModifiedID", typeof(Int32));

                // Set the primary key. 
                table.Columns["ProviderName"].Unique = true;
                table.PrimaryKey = new DataColumn[] { table.Columns["ProviderName"] };

                table.Columns["DocTBGuid"].Unique = true;
                table.PrimaryKey = new DataColumn[] { table.Columns["DocTBGuid"] };

                return table;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private DataTable DS_ValidationInfo { get; set; }
        protected SimpleValidationDBManager(int maxCapacityCache)
        {
            DS_ValidationInfo = MakeDS_ValidationInfoDataTable();
            MaxCapacityCache = maxCapacityCache;
        }

        public static int MaxCapacityCache { get; private set; }

        public bool ExistRecordInValidationInfo(TBConnection myConnection, string docTBGuid, string providerName, string whereProviderName)
        {
            lock (_locker)
            {
                using (TBCommand myCommand = new TBCommand(myConnection))
                    return ExistRecordInValidationInfoInDataTable(docTBGuid, providerName) || ExistRecordInValidationInfoInDB(myCommand, docTBGuid, providerName, whereProviderName);
            }
        }

        private bool ExistRecordInValidationInfoInDB(TBCommand myCommand, string docTBGuid, string providerName, string whereProviderName)
        {
            myCommand.Parameters.Clear();
            myCommand.CommandText = @"SELECT COUNT(*) FROM [DS_ValidationInfo] WHERE [DocTBGuid] = @DocTBGuid" + whereProviderName;

            myCommand.Parameters.Add("@providerName", providerName);
            myCommand.Parameters.Add("@DocTBGuid", docTBGuid);
            int nRows = Convert.ToInt32(myCommand.ExecuteScalar());
            if (nRows >= 1)
                return true;
            else return false;
        }

        private bool ExistRecordInValidationInfoInDataTable(string docTBGuid, string providerName)
        {
            DataRow[] result = DS_ValidationInfo.Select($"DocTBGuid = '{docTBGuid}' AND ProviderName = '{providerName}'");
            if (result == null || result.Count() == 0)
                return false;
            return true;
        }

        private void DoBulkInsert(string connectionString)
        {
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connectionString))
            {
                bulkCopy.DestinationTableName = "DS_ValidationInfo";

                try
                {
                    bulkCopy.WriteToServer(DS_ValidationInfo);
                    DS_ValidationInfo.Clear();

                    if (ValidationErrorIsEmpty)
                        ValidationErrorIsEmpty = false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public int DeleteValidationInfo(TBConnection myConnection, string providerName)
        {
            lock (_locker)
            {
                using (TBCommand myCommand = new TBCommand(myConnection))
                {
                    myCommand.Parameters.Clear();
                    myCommand.CommandText = "DELETE FROM [DS_ValidationInfo] WHERE [ProviderName] = @providerName";
                    myCommand.Parameters.Add("@providerName", providerName);
                    int nRows = myCommand.ExecuteNonQuery();

                    ValidationErrorIsEmpty = true;

                    return nRows;
                }                    
            }
        }
        public int DeleteValidationInfo(TBConnection myConnection, string providerName, string docTBGuid)
        {
            lock (_locker)
            {
                using (TBCommand myCommand = new TBCommand(myConnection))
                {
                    myCommand.Parameters.Clear();
                    myCommand.CommandText = "DELETE FROM [DS_ValidationInfo] WHERE [ProviderName] = @providerName AND [DocTBGuid] = @docTBGuid";
                    myCommand.Parameters.Add("@DocTBGuid", docTBGuid);
                    myCommand.Parameters.Add("@providerName", providerName);
                    int nRows = myCommand.ExecuteNonQuery();
                    return nRows;
                }  
            }
        }

        public int InsertValidationInfo(TBConnection myConnection, string docTBGuid, string actionName, string docNamespace, DateTime validationDate, IAFError dataError, int workerID, bool bUsedForFilter, string providerName, string whereProviderName)
        {
            lock (_locker)
            {
                if (DS_ValidationInfo.Rows.Count == MaxCapacityCache)
                    DoBulkInsert(myConnection.ConnectionString);
                DateTime now = DateTime.Now;

                DataRow row = DS_ValidationInfo.NewRow();
                row["DocTBGuid"] = docTBGuid;
                row["ProviderName"] = providerName;
                row["DocNamespace"] = docNamespace;
                row["FKError"] = dataError.IsFKError ? "1" : "0";
                row["XSDError"] = dataError.IsXSDError ? "1" : "0";
                row["UsedForFilter"] = bUsedForFilter ? "1" : "0";
                row["MessageError"] = dataError.MessageError;
                row["ActionName"] = actionName;
                row["ValidationDate"] = validationDate;
                row["TBCreated"] = now;
                row["TBModified"] = now;
                row["TBCreatedID"] = workerID;
                row["TBModifiedID"] = workerID;
                DS_ValidationInfo.Rows.Add(row);
                DS_ValidationInfo.AcceptChanges();

                return 1;
            }
        }

        public int UpdateValidationInfo(TBConnection myConnection, string docTBGuid, string actionName, string docNamespace, DateTime validationDate, IAFError dataError, int workerID, string providerName, string whereProviderName)
        {
            lock (_locker)
            {
                if (ExistRecordInValidationInfoInDataTable(docTBGuid, providerName))
                {
                    DataRow[] result = DS_ValidationInfo.Select($"DocTBGuid = '{docTBGuid}'");
                    if (dataError.IsFKError)
                        result[0]["FKError"] = "1";
                    else
                        result[0]["XSDError"] = "1";

                    result[0]["MessageError"] = dataError.MessageError;

                    result[0]["ActionName"] = actionName;

                    result[0]["ValidationDate"] = validationDate;
                    result[0]["TBModified"] = DateTime.Now;
                    result[0]["TBModifiedID"] = workerID;
                    return 1;
                }
                else
                {
                    using (TBCommand myCommand = new TBCommand(myConnection))
                    {
                        myCommand.Parameters.Clear();

                        if (dataError.IsFKError)
                            myCommand.CommandText = @"UPDATE [DS_ValidationInfo] 
                                                            SET [MessageError] = @MessageError, [ActionName] = @ActionName, 
                                                            [DocNamespace] = @DocNamespace, [FKError] = @FKError,
                                                            [ValidationDate] = @ValidationDate, [TBModified] = @TBModified
                                                            WHERE [DocTBGuid] = @DocTBGuid" + whereProviderName;
                        else
                            myCommand.CommandText = @"UPDATE [DS_ValidationInfo] 
                                                            SET [MessageError] = @MessageError, [ActionName] = @ActionName, 
                                                            [DocNamespace] = @DocNamespace, [XSDError] = @XSDError, 
                                                            [ValidationDate] = @ValidationDate, [TBModified] = @TBModified
                                                            WHERE [DocTBGuid] = @DocTBGuid" + whereProviderName;
                        myCommand.Parameters.Add("@providerName", providerName);
                        myCommand.Parameters.Add("@DocTBGuid", docTBGuid);
                        myCommand.Parameters.Add("@DocNamespace", docNamespace);
                        if (dataError.IsFKError)
                            myCommand.Parameters.Add("@FKError", dataError.IsFKError);
                        else
                            myCommand.Parameters.Add("@XSDError", dataError.IsXSDError);

                        myCommand.Parameters.Add("@MessageError", dataError.MessageError);
                        myCommand.Parameters.Add("@ActionName", actionName);
                        myCommand.Parameters.Add("@ValidationDate", validationDate);
                        myCommand.Parameters.Add("@TBModified", DateTime.Now);
                        myCommand.Parameters.Add("@TBModifiedID", workerID);
                        return myCommand.ExecuteNonQuery();
                    }
                }
            }
        }

        public bool ValidationErrorIsEmpty { get; private set; }

        public void DeleteFKToFixRecordsForMassiveValidation(TBConnection myConnection, string providerName)
        {
            lock (_locker)
            {
                using (TBCommand myCommand = new TBCommand(myConnection))
                {
                    myCommand.Parameters.Clear();
                    myCommand.CommandText = "DELETE FROM [DS_ValidationFKToFix] WHERE [ProviderName] = @providerName";
                    myCommand.Parameters.Add("@providerName", providerName);
                    myCommand.ExecuteNonQuery();
                }                   
            }
        }

        public bool ExistRecordInValidationFKtoFix(TBConnection myConnection, string providerName, string docNamespace, string qualifiedField, string value, string whereProviderName)
        {
            lock (_locker)
            {
                using (TBCommand myCommand = new TBCommand(myConnection))
                {
                    bool bRecordExist = false;

                    string table = string.Empty;
                    string field = string.Empty;

                    int pointIndex = qualifiedField.IndexOf(".");
                    table = qualifiedField.Substring(0, pointIndex);
                    field = qualifiedField.Substring(pointIndex + 1);

                    // controllo se esiste nella DS_SynchronizationErrors una riga con quel TBGuid
                    string queryexistInfo = "SELECT COUNT(*) FROM [DS_ValidationFKtoFix] WHERE [DocNamespace] = @docNamespace AND [TableName] = @TableName AND [FieldName] = @FieldName AND [ValueToFix] = @ValueToFix" + whereProviderName;

                    myCommand.Parameters.Clear();
                    myCommand.CommandText = queryexistInfo;
                    myCommand.Parameters.Add("@DocNamespace", docNamespace);
                    myCommand.Parameters.Add("@TableName", table);
                    myCommand.Parameters.Add("@FieldName", field);
                    myCommand.Parameters.Add("@ValueToFix", value);
                    myCommand.Parameters.Add("@providerName", providerName);

                    bRecordExist = (int)myCommand.ExecuteScalar() > 0;

                    return bRecordExist;
                }
            }
        }

        public void InsertValidationFKtoFix(TBConnection myConnection, string providerName, string docNamespace, string qualifiedField, string value, DateTime validationDate, int workerID, string whereProviderName)
        {
            lock (_locker)
            {
                using (TBCommand myCommand = new TBCommand(myConnection))
                {
                    bool bRecordExist = false;

                    FKToFixInfo info = new FKToFixInfo(providerName, docNamespace, qualifiedField, value);

                    string table = string.Empty;
                    string field = string.Empty;

                    int pointIndex = qualifiedField.IndexOf(".");
                    table = qualifiedField.Substring(0, pointIndex);
                    field = qualifiedField.Substring(pointIndex + 1);

                    bRecordExist = ExistRecordInValidationFKtoFix(myConnection, providerName, docNamespace, qualifiedField, value, whereProviderName);

                    if (bRecordExist)
                    {
                        Factory.Instance.GetFKToFixErrors().Update(info);
                        return;
                    }

                    Factory.Instance.GetFKToFixErrors().Add(info);

                    myCommand.Parameters.Clear();
                    myCommand.CommandText = @"INSERT INTO [DS_ValidationFKtoFix] 
                                        ([ProviderName], [DocNamespace], [TableName], [FieldName], [ValueToFix], [ValidationDate], [TBCreated], [TBModified], [TBCreatedID], [TBModifiedID]) 
									    VALUES (@ProviderName, @DocNamespace, @TableName, @FieldName, @ValueToFix, @ValidationDate, @TBCreated, @TBModified, @TBCreatedID, @TBModifiedID)";

                    myCommand.Parameters.Add("@providerName", providerName);
                    myCommand.Parameters.Add("@DocNamespace", docNamespace);
                    myCommand.Parameters.Add("@TableName", table);
                    myCommand.Parameters.Add("@FieldName", field);
                    myCommand.Parameters.Add("@ValueToFix", value);
                    myCommand.Parameters.Add("@ValidationDate", validationDate);
                    myCommand.Parameters.Add("@TBCreated", DateTime.Now);
                    myCommand.Parameters.Add("@TBModified", DateTime.Now);
                    myCommand.Parameters.Add("@TBCreatedID", workerID);
                    myCommand.Parameters.Add("@TBModifiedID", workerID);

                    myCommand.ExecuteNonQuery();
                }
            }
        }

        public void UpdateValidationFKtoFixWithRelatedErrors(TBConnection myConnection, string providerName, string whereProviderName)
        {
            lock (_locker)
            {
                bool bRecordExist = false;

                string table = string.Empty;
                string field = string.Empty;
                int pointIndex = 0;

                using (TBCommand myCommand = new TBCommand(myConnection))
                {
                    foreach (IFKToFixInfo key in Factory.Instance.GetFKToFixErrors())
                    {
                        bRecordExist = ExistRecordInValidationFKtoFix(myConnection, providerName, key.DocNamespace, key.QualifiedField, key.Value, whereProviderName);

                        pointIndex = key.QualifiedField.IndexOf(".");
                        table = key.QualifiedField.Substring(0, pointIndex);
                        field = key.QualifiedField.Substring(pointIndex + 1);

                        if (!bRecordExist)
                            continue;

                        myCommand.Parameters.Clear();
                        myCommand.CommandText = @"UPDATE [DS_ValidationFKtoFix] SET [RelatedErrors] = @RelatedErrors, 
                                                                                    [TBModified]    = @TBModified, 
                                                                                    [TBModifiedID]  = @TBModifiedID
                                                                                WHERE [DocNamespace]  = @DocNamespace AND
                                                                                    [TableName]     = @TableName    AND
                                                                                    [FieldName]     = @FieldName    AND
                                                                                    [ValueToFix]    = @ValueToFix"
                                                                                        + whereProviderName;

                        myCommand.Parameters.Clear();
                        myCommand.Parameters.Add("@RelatedErrors", Factory.Instance.GetFKToFixErrors().Get(key));
                        myCommand.Parameters.Add("@TBModified", DateTime.Now);
                        myCommand.Parameters.Add("@TBModifiedID", 0);
                        myCommand.Parameters.Add("@DocNamespace", key.DocNamespace);
                        myCommand.Parameters.Add("@TableName", table);
                        myCommand.Parameters.Add("@FieldName", field);
                        myCommand.Parameters.Add("@ValueToFix", key.Value);
                        myCommand.Parameters.Add("@providerName", providerName);

                        myCommand.ExecuteNonQuery();
                    }
                }
                    
            }
        }
    }
}


