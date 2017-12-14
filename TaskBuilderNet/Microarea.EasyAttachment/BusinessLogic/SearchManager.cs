using Microarea.EasyAttachment.Components;
using Microarea.EasyAttachment.Core;
using Microarea.Framework.TBApplicationWrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

/*Esempio di query:
Reange di date
Free Tag: doc relational
Field Search: Colore = Rosso corrispondente al SearchIndexID  75

((SELECT Distinct D.ArchivedDocID, D.Name, D.Description, D.TBModified, D.TBCreated, D.TBModifiedID, D.ModifierID, D.StorageType, D.IsWoormReport 
FROM DMS_ArchivedDocument D , DMS_ArchivedDocSearchIndexes I, DMS_SearchFieldIndexes S 
WHERE ((D.TBModified BETWEEN '2015-10-01 00:00:00' AND '2015-11-16 23:59:59' ) OR (D.TBCreated BETWEEN '2015-10-01 00:00:00' AND '2015-11-16 23:59:59')) 
AND D.ArchivedDocID = I.ArchivedDocID AND S.SearchIndexID = I.SearchIndexID AND S.FieldValue LIKE '%doc%' ) 
INTERSECT 
( SELECT Distinct D.ArchivedDocID, D.Name, D.Description, D.TBModified, D.TBCreated, D.TBModifiedID, D.ModifierID, D.StorageType, D.IsWoormReport 
FROM DMS_ArchivedDocument D , DMS_ArchivedDocSearchIndexes I, DMS_SearchFieldIndexes S 
WHERE ((D.TBModified BETWEEN '2015-10-01 00:00:00' AND '2015-11-16 23:59:59' ) OR (D.TBCreated BETWEEN '2015-10-01 00:00:00' AND '2015-11-16 23:59:59')) 
AND D.ArchivedDocID = I.ArchivedDocID AND S.SearchIndexID = I.SearchIndexID AND S.FieldValue LIKE '%relational%' ) 
UNION 
( SELECT Distinct D.ArchivedDocID, D.Name, D.Description, D.TBModified, D.TBCreated, D.TBModifiedID, D.ModifierID, D.StorageType, D.IsWoormReport 
FROM DMS_ArchivedDocument D , DMS_ArchivedDocContent C 
WHERE ((D.TBModified BETWEEN '2015-10-01 00:00:00' AND '2015-11-16 23:59:59' ) OR (D.TBCreated BETWEEN '2015-10-01 00:00:00' AND '2015-11-16 23:59:59')) 
AND D.ArchivedDocID = C.ArchivedDocID AND(C.OCRProcess = 1 AND CONTAINS(C.BinaryContent, 'doc') AND CONTAINS(C.BinaryContent, 'relational') ) 
UNION 
SELECT Distinct D.ArchivedDocID, D.Name, D.Description, D.TBModified, D.TBCreated, D.TBModifiedID, D.ModifierID, D.StorageType, D.IsWoormReport 
FROM DMS_ArchivedDocument D , DMS_ArchivedDocContent C, DMS_ArchivedDocTextContent T 
WHERE ((D.TBModified BETWEEN '2015-10-01 00:00:00' AND '2015-11-16 23:59:59' ) OR (D.TBCreated BETWEEN '2015-10-01 00:00:00' AND '2015-11-16 23:59:59')) 
AND (D.ArchivedDocID = C.ArchivedDocID AND C.ArchivedDocID = T.ArchivedDocID) AND(C.OCRProcess = 1 AND CONTAINS(T.TextContent, 'doc') AND CONTAINS(T.TextContent, 'relational') ) ))

INTERSECT 
(
	( SELECT Distinct D.ArchivedDocID, D.Name, D.Description, D.TBModified, D.TBCreated, D.TBModifiedID, D.ModifierID, D.StorageType, D.IsWoormReport 
	 FROM DMS_ArchivedDocument D , DMS_ArchivedDocSearchIndexes I 
	 WHERE ((D.TBModified BETWEEN '2015-10-01 00:00:00' AND '2015-11-16 23:59:59' ) OR (D.TBCreated BETWEEN '2015-10-01 00:00:00' AND '2015-11-16 23:59:59')) 
	 AND D.ArchivedDocID = I.ArchivedDocID AND SearchIndexID = 75 ) 
)*/
namespace Microarea.EasyAttachment.BusinessLogic
{
    public class IndexToSynchro
    {
        public string FieldName = string.Empty;
        public string FieldValue = string.Empty;
        public string FormattedValue = string.Empty;
    };

    //================================================================================
    public class SearchManager : BaseManager
    {
        //create an analyzer to process the text
        DMSModelDataContext dc = null;

        FilterEventArgs fea = null;
        SqlCommand sqlCommand = null;

		private string archivedDocBaseSelected = string.Empty;
		private string attachmentBaseSelected = string.Empty;
		private string archivedDocGroupBy = string.Empty;

		//datamenber used for thread
		private bool startSearchThread = false;
		private string docNamespacesToProtect = string.Empty;		
			//security integration
		ERPDocumentsGrant erpDocumentsGrant = null;

        // for each search field contains all possible values contained in all extracted attachements (stored in extracted searchResultDT)
        private SearchFieldsDataTable completeBookmarksList = new SearchFieldsDataTable();
        private SearchFieldsDataTable mostUsedBookMarks = new SearchFieldsDataTable();

        public SearchRules SearchRules = null;

        //Properties
        //-----------------------------------------------------------------------------------------------
        public DMSModelDataContext DataContext { get { return dc; } }

        //Events
        //-----------------------------------------------------------------------------------------------
        public event EventHandler<SearchResultAddingDataTableEventArgs> SearchResultAddingDataTable;
        public event EventHandler SearchResultCleared;
        public event EventHandler SearchFinished;
        public event EventHandler SyncronizationIndexesFinished;
        public event EventHandler<AddRowInResultEventArgs> OnAddRowToResult;


        ///<summary> 
        /// Constructor
        ///</summary>
        //-----------------------------------------------------------------------------------------------
        public SearchManager(DMSOrchestrator orchestrator)
        {
            DMSOrchestrator = orchestrator;
            ManagerName = "SearchManager";

            dc = DMSOrchestrator.DataContext;
			string basefields = "D.ArchivedDocID, D.Name, D.Description, D.TBModified, D.TBCreated, D.TBModifiedID, D.ModifierID, D.StorageType, D.IsWoormReport";
			archivedDocBaseSelected = "SELECT Distinct " + basefields + ", hasAttach = CASE WHEN count(A.ArchivedDocID) <= 0 THEN 0 ELSE 1  END  FROM DMS_ArchivedDocument D LEFT OUTER JOIN DMS_Attachment A ON D.ArchivedDocID = A.ArchivedDocID";
			attachmentBaseSelected = "SELECT Distinct " + basefields + ", 1 as hasAttach FROM DMS_ArchivedDocument D, DMS_Attachment A ";
			archivedDocGroupBy = " GROUP BY " + basefields;


			SearchRules = new SearchRules(DMSOrchestrator.CollectorName, DMSOrchestrator.CollectionName);
        }

        //-----------------------------------------------------------------------------------------------
        private int AddSearchTerm(string name, string term, string formattedValue)
        {
            try
            {
                var var = from searchField in dc.DMS_SearchFieldIndexes
                          where searchField.FieldName == name && searchField.FieldValue == term
                          select searchField.SearchIndexID;

                int fieldIdx = (var != null && var.Any()) ? (int)var.Single() : -1;

                if (fieldIdx == -1)
                {
                    DMS_SearchFieldIndex field = new DMS_SearchFieldIndex();
                    field.FieldName = name;
                    field.FieldValue = term;
                    field.FormattedValue = formattedValue;
                    dc.DMS_SearchFieldIndexes.InsertOnSubmit(field);
                    dc.SubmitChanges();
                    fieldIdx = field.SearchIndexID;
                }
                return fieldIdx;
            }
            catch (Exception e)
            {
                throw (e);
            }
        }
        //-----------------------------------------------------------------------------------------------
        private void AddSearchIndex(AttachmentInfo attachmentInfo, string name, string term, string formattedValue)
        {
            if (string.IsNullOrWhiteSpace(name) ||
                    (
                        !DMSOrchestrator.SettingsManager.UsersSettingState.Options.BookmarksOptionsState.EnableEmptyValues &&
                        string.IsNullOrEmpty(term)
                    ))
                return;

            try
            {
                int fieldIdx = AddSearchTerm(name, term, formattedValue);
                if (attachmentInfo.AttachmentId > -1 && fieldIdx > -1)
                {
                    //add the link between the value term and the attachment
                    DMS_AttachmentSearchIndex attSearchIndex = new DMS_AttachmentSearchIndex();
                    attSearchIndex.AttachmentID = attachmentInfo.AttachmentId;
                    attSearchIndex.SearchIndexID = fieldIdx;
                    dc.DMS_AttachmentSearchIndexes.InsertOnSubmit(attSearchIndex);
                    dc.SubmitChanges();
                }
                else
                {
                    if (fieldIdx > -1)
                    {
                        //add the link between the value term and the attachment
                        DMS_ArchivedDocSearchIndex searchIndex = new DMS_ArchivedDocSearchIndex();
                        searchIndex.ArchivedDocID = attachmentInfo.ArchivedDocId;
                        searchIndex.SearchIndexID = fieldIdx;
                        dc.DMS_ArchivedDocSearchIndexes.InsertOnSubmit(searchIndex);
                        dc.SubmitChanges();
                    }
                }
            }
            catch (Exception e)
            {
                throw (e);
            }
        }
        //-----------------------------------------------------------------------------------------------
        private int GetSearchFieldId(SqlConnection dmsConnection, string name, string term, string formattedValue)
        {
            SqlCommand selSearchFieldCmd = new SqlCommand();
            int searchFieldId = -1;

            try
            {
                selSearchFieldCmd = new SqlCommand(string.Format(@"SELECT SearchIndexID FROM DMS_SearchFieldIndexes where FieldName = '{0}' AND FieldValue = @Term", name), dmsConnection);
				selSearchFieldCmd.Parameters.AddWithValue("@Term", term);
				object obj = selSearchFieldCmd.ExecuteScalar();
                searchFieldId = (obj != null) ? (int)obj : -1;

                if (searchFieldId == -1)
                {
                    selSearchFieldCmd.CommandText = string.Format(@"INSERT INTO DMS_SearchFieldIndexes ( FieldName, FieldValue ) VALUES('{0}', @Term ); SELECT scope_identity();", name);
                    searchFieldId = System.Convert.ToInt32(selSearchFieldCmd.ExecuteScalar());
                }

            }
            catch (SqlException e)
            {
                throw (e);
            }
            catch (Exception e)
            {
                throw (e);
            }

            finally
            {
                if (selSearchFieldCmd != null)
                    selSearchFieldCmd.Dispose();
            }

            return searchFieldId;
        }


        //-----------------------------------------------------------------------------------------------
        private void AddSearchIndex(SqlConnection dmsConnection, AttachmentInfo attachmentInfo, string name, string term, string formattedValue)
        {
            if (string.IsNullOrWhiteSpace(name) ||
                    (
                        !DMSOrchestrator.SettingsManager.UsersSettingState.Options.BookmarksOptionsState.EnableEmptyValues &&
                        string.IsNullOrEmpty(term)
                    ))
                return;

            SqlCommand insertIdx = null;
            int fieldIdx = -1;
            try
            {
                insertIdx = new SqlCommand(string.Empty, dmsConnection);
				insertIdx.CommandText = @"SELECT SearchIndexID FROM DMS_SearchFieldIndexes where FieldName = @Name AND FieldValue = @Term";
				insertIdx.Parameters.AddWithValue("@Name", name);
				insertIdx.Parameters.AddWithValue("@Term", term);
                object obj = insertIdx.ExecuteScalar();
                fieldIdx = (obj != null) ? (int)obj : -1;

                if (fieldIdx == -1)
                {
					insertIdx.CommandText = @"INSERT INTO DMS_SearchFieldIndexes (FieldName, FieldValue, FormattedValue) VALUES (@Name, @Term, @FormattedValue); SELECT scope_identity();";
					insertIdx.Parameters.AddWithValue("@FormattedValue", formattedValue);
                    fieldIdx = System.Convert.ToInt32(insertIdx.ExecuteScalar());
                }

                if (fieldIdx == -1)
                    return;

                if (attachmentInfo.AttachmentId > -1)
                {
                    insertIdx.CommandText = string.Format(@"INSERT INTO DMS_AttachmentSearchIndexes (AttachmentID, SearchIndexID) VALUES( {0}, {1} )", attachmentInfo.AttachmentId, fieldIdx);
                    insertIdx.ExecuteNonQuery();
                }
                else
                {
                    insertIdx.CommandText = string.Format(@"INSERT INTO DMS_ArchivedDocSearchIndexes (ArchivedDocID, SearchIndexID) VALUES( {0}, {1} )", attachmentInfo.ArchivedDocId, fieldIdx);
                    insertIdx.ExecuteNonQuery();
                }
            }


            catch (SqlException e)
            {
                throw (e);
            }
            catch (Exception e)
            {
                throw (e);
            }
            finally
            {
                if (insertIdx != null)
                    insertIdx.Dispose();
            }
        }

        
        /// <summary>
        /// write all indexes of a single attachment
        /// </summary>			
        //-----------------------------------------------------------------------------------------------
        public void InsertIndexes(AttachmentInfo attachmentInfo, string freeTag, string description)
        {
            try
            {
                if (attachmentInfo.BookmarksDT != null)
                    foreach (DataRow field in attachmentInfo.BookmarksDT.Rows)
                    {
                        if (field.RowState == DataRowState.Deleted)
                            continue;
                        string name = (string)field[CommonStrings.Name];
                        string value = ((FieldData)field[CommonStrings.FieldData]).StringValue;
                        string formattedValue = ((FieldData)field[CommonStrings.FieldData]).FormattedValue;
                        AddSearchIndex(attachmentInfo, name, value, formattedValue);
                    }


                if (!string.IsNullOrWhiteSpace(freeTag))
                    AddSearchIndex(attachmentInfo, CommonStrings.Tags, freeTag, freeTag);

                if (!string.IsNullOrWhiteSpace(attachmentInfo.Name))
                    AddSearchIndex(attachmentInfo, CommonStrings.FileNameTag, attachmentInfo.Name, attachmentInfo.Name);

                if (!string.IsNullOrWhiteSpace(attachmentInfo.TBarcode.Value))
                    AddSearchIndex(attachmentInfo, CommonStrings.BarcodeTag, attachmentInfo.TBarcode.Value, attachmentInfo.TBarcode.Value);

                //I insert the DescriptionTag index only if the description is differt by file name (at beginning are equals)
                if (
                    !string.IsNullOrWhiteSpace(description) &&
                    string.Compare(attachmentInfo.Name, description, StringComparison.InvariantCultureIgnoreCase) != 0
                    )
                    AddSearchIndex(attachmentInfo, CommonStrings.DescriptionTag, description, description);

            }
            catch (Exception e)
            {
                SetMessage(Strings.ErrorWritingIndexes, e, "InsertIndexes");
            }
        }


        /// <summary>
        /// write all indexes of a single attachment
        /// </summary>			
        //-----------------------------------------------------------------------------------------------
        public void InsertIndexesSql(AttachmentInfo attachmentInfo, string freeTag, string description)
        {
            //bool insertIndexResult = false;
            SqlConnection dmsConnection = null;
            //SqlTransaction transaction = null;
            try
            {
                dmsConnection = new SqlConnection(DMSOrchestrator.DMSConnectionString);
                dmsConnection.Open();
                //transaction = dmsConnection.BeginTransaction();
                if (attachmentInfo.BookmarksDT != null)
                    foreach (DataRow field in attachmentInfo.BookmarksDT.Rows)
                    {
                        if (field.RowState == DataRowState.Deleted)
                            continue;
                        string name = (string)field[CommonStrings.Name];
                        string value = ((FieldData)field[CommonStrings.FieldData]).StringValue;
                        string formattedValue = ((FieldData)field[CommonStrings.FieldData]).FormattedValue;
                        AddSearchIndex(dmsConnection, attachmentInfo, name, value, formattedValue);
                    }

                if (!string.IsNullOrWhiteSpace(freeTag))
                    AddSearchIndex(dmsConnection, attachmentInfo, CommonStrings.Tags, freeTag, freeTag);

                if (!string.IsNullOrWhiteSpace(attachmentInfo.Name))
                    AddSearchIndex(dmsConnection, attachmentInfo, CommonStrings.FileNameTag, attachmentInfo.Name, attachmentInfo.Name);

                if (!string.IsNullOrWhiteSpace(attachmentInfo.TBarcode.Value))
                    AddSearchIndex(dmsConnection, attachmentInfo, CommonStrings.BarcodeTag, attachmentInfo.TBarcode.Value, attachmentInfo.TBarcode.Value);


                //I insert the DescriptionTag index only if the description is differt by file name (at beginning are equals)
                if (
                    !string.IsNullOrWhiteSpace(description) &&
                    string.Compare(attachmentInfo.Name, description, StringComparison.InvariantCultureIgnoreCase) != 0
                    )
                    AddSearchIndex(dmsConnection, attachmentInfo, CommonStrings.DescriptionTag, description, description);
            }
            catch (SqlException e)
            {
                SetMessage(Strings.ErrorWritingIndexes, e, "InsertIndexes");
            }
            catch (Exception e)
            {
                SetMessage(Strings.ErrorWritingIndexes, e, "InsertIndexes");
            }
            finally
            {
                if (dmsConnection != null && dmsConnection.State == ConnectionState.Open)
                {
                    dmsConnection.Close();
                    dmsConnection.Dispose();
                }
            }
        }

        //---------------------------------------------------------------------------------------
        private DMS_SearchFieldIndex GetSearchField(string name, string value)
        {
            var var = from searchField in dc.DMS_SearchFieldIndexes
                      where searchField.FieldName == name && searchField.FieldValue == value
                      select searchField;

            return (var != null && var.Any()) ? (DMS_SearchFieldIndex)var.Single() : null;
        }

        //-----------------------------------------------------------------------------------------------
        private void UpdateAttachmentIndexes(AttachmentInfo attachmentInfo, string freeTag, string description)
        {
            try
            {
                int attachmentID = attachmentInfo.AttachmentId;
                var var = from attIndexes in dc.DMS_AttachmentSearchIndexes
                          where attIndexes.AttachmentID == attachmentID
                          select attIndexes;

                if (var == null || !var.Any())
                    InsertIndexes(attachmentInfo, freeTag, description);
                else
                {
                    //first I delete the search indexes of attachmentID
                    dc.DMS_AttachmentSearchIndexes.DeleteAllOnSubmit(var);
                    dc.SubmitChanges();

                    //then I insert the new search indexes
                    InsertIndexes(attachmentInfo, freeTag, description);
                }
            }
            catch (Exception e)
            {
                SetMessage(Strings.ErrorUpdatingSearchIndexes, e, "UpdateAttachmentIndexes");
            }
        }

        //-----------------------------------------------------------------------------------------------
        private void UpdateArchiveDocIndexes(AttachmentInfo attachmentInfo, string freeTag, string description)
        {
            try
            {
                int archivedDocId = attachmentInfo.ArchivedDocId;
                var var = from attIndexes in dc.DMS_ArchivedDocSearchIndexes
                          where attIndexes.ArchivedDocID == archivedDocId
                          select attIndexes;

                if (var == null || !var.Any())
                    InsertIndexes(attachmentInfo, freeTag, description);
                else
                {
                    //first I delete the search indexes of attachmentID
                    dc.DMS_ArchivedDocSearchIndexes.DeleteAllOnSubmit(var);
                    dc.SubmitChanges();

                    //then I insert the new search indexes
                    InsertIndexes(attachmentInfo, freeTag, description);
                }
            }
            catch (Exception e)
            {
                SetMessage(Strings.ErrorUpdatingSearchIndexes, e, "UpdateArchiveDocIndexes");
            }
        }

        ///<summary>
        /// Aggiorno la tabella degli indici del documento archiviato relativamente al field BarcodeTag
        ///</summary>
        //-----------------------------------------------------------------------------------------------
        public void UpdateBarcodeIndex(AttachmentInfo attachmentInfo, string barcodeValue)
        {
            try
            {
                // seleziono l'indice solo del tipo BarcodeTag
                var var = from archDocIndexes in dc.DMS_ArchivedDocSearchIndexes
                          where archDocIndexes.ArchivedDocID == attachmentInfo.ArchivedDocId &&
                          archDocIndexes.DMS_SearchFieldIndex.FieldName == CommonStrings.BarcodeTag
                          select archDocIndexes;

                // se non esiste lo aggiungo
                if (var == null || !var.Any())
                    AddSearchIndexForBarcode(attachmentInfo.ArchivedDocId, barcodeValue);
                else
                {
                    // prima elimino gli indici di ricerca dell'archivedoc
                    dc.DMS_ArchivedDocSearchIndexes.DeleteAllOnSubmit(var);
                    dc.SubmitChanges();

                    // dopo inserisco il nuovo indice
                    AddSearchIndexForBarcode(attachmentInfo.ArchivedDocId, barcodeValue);
                }
            }
            catch (Exception e)
            {
                SetMessage(Strings.ErrorUpdatingSearchIndexes, e, "UpdateBarcodeIndex");
            }
        }

        ///<summary>
        /// Aggiunge, se necessario, una riga nella tabella degli indici del documento archiviato relativamente al field BarcodeTag
        ///</summary>
        //-----------------------------------------------------------------------------------------------
        private void AddSearchIndexForBarcode(int archivedDocId, string barcodeValue)
        {
            if (string.IsNullOrWhiteSpace(barcodeValue))
                return;

            try
            {
                int fieldIdx = AddSearchTerm(CommonStrings.BarcodeTag, barcodeValue, barcodeValue);
                if (fieldIdx > -1)
                {

                    //add the link between the value term and the attachment
                    DMS_ArchivedDocSearchIndex searchIndex = new DMS_ArchivedDocSearchIndex();
                    searchIndex.ArchivedDocID = archivedDocId;
                    searchIndex.SearchIndexID = fieldIdx;
                    dc.DMS_ArchivedDocSearchIndexes.InsertOnSubmit(searchIndex);
                    dc.SubmitChanges();
                }
            }
            catch (Exception e)
            {
                throw (e);
            }
        }

        /// <summary>
        /// update indexes of an existing attachment
        /// </summary>
        //-----------------------------------------------------------------------------------------------
        public void UpdateIndexes(AttachmentInfo attachmentInfo, string freeTag, string description)
        {
            try
            {
                if (attachmentInfo.AttachmentId > -1)
                    UpdateAttachmentIndexes(attachmentInfo, freeTag, description);
                else
                    UpdateArchiveDocIndexes(attachmentInfo, freeTag, description);
            }
            catch (Exception e)
            {
                SetMessage(Strings.ErrorUpdatingSearchIndexes, e, "UpdateIndexes");
            }
        }

        //remove the attachment's indexes
        //-----------------------------------------------------------------------------------------------
        public void DeleteAttachmentIndexes(int attachmentId)
        {
            try
            {
                var var = from attIndexes in dc.DMS_AttachmentSearchIndexes
                          where attIndexes.AttachmentID == attachmentId
                          select attIndexes;

                if (var != null && var.Any())
                {
                    foreach (DMS_AttachmentSearchIndex oldIndex in var)
                    {
                        DMS_SearchFieldIndex searchField = oldIndex.DMS_SearchFieldIndex;
                        dc.DMS_AttachmentSearchIndexes.DeleteOnSubmit(oldIndex);
                        dc.SubmitChanges();
                    }
                }
            }
            catch (Exception e)
            {
                SetMessage(Strings.ErrorWritingIndexes, e, "DeleteAttachmentIndexes");
            }
        }

        //remove the archived document's indexes
        //-----------------------------------------------------------------------------------------------
        public void DeleteArchivedDocIndexes(int archivedDocId)
        {
            try
            {
                var var = from attIndexes in dc.DMS_ArchivedDocSearchIndexes
                          where attIndexes.ArchivedDocID == archivedDocId
                          select attIndexes;

                if (var != null && var.Any())
                {
                    foreach (DMS_ArchivedDocSearchIndex oldIndex in var)
                    {
                        DMS_SearchFieldIndex searchField = oldIndex.DMS_SearchFieldIndex;
                        dc.DMS_ArchivedDocSearchIndexes.DeleteOnSubmit(oldIndex);
                        dc.SubmitChanges();
                    }
                }

            }
            catch (Exception e)
            {
                SetMessage(Strings.ErrorWritingIndexes, e, "DeleteArchivedDocIndexes");
            }
        }

        /// <summary>
        /// delete indexes of a removed attachment/archiveddocument
        /// </summary>
        //-----------------------------------------------------------------------------------------------
        public void DeleteIndexes(AttachmentInfo attachmentInfo)
        {
            if (attachmentInfo.AttachmentId > -1)
                DeleteAttachmentIndexes(attachmentInfo.AttachmentId);
            else
                DeleteArchivedDocIndexes(attachmentInfo.ArchivedDocId);
        }


        //-----------------------------------------------------------------------------------------------
        private void GetArchivedDocBookmarksValues(ref AttachmentInfo attachmentInfo)
        {
            try
            {
                //dc.Refresh(RefreshMode.OverwriteCurrentValues, dc.DMS_ArchivedDocSearchIndexes);

                int archivedDocId = attachmentInfo.ArchivedDocId;
                var var = from attIndexes in dc.DMS_ArchivedDocSearchIndexes
                          where attIndexes.ArchivedDocID == archivedDocId
                          select attIndexes;

                if (var != null && var.Any())
                {
                    DataRow row = null;
                    DMS_ArchivedDocSearchIndex attachmentIndex = null;

                    for (int i = attachmentInfo.BookmarksDT.Rows.Count - 1; i >= 0; i--)
                    {
                        row = attachmentInfo.BookmarksDT.Rows[i];
                        if (row.RowState == DataRowState.Deleted)
                            continue;

                        if (var.Any(f => f.DMS_SearchFieldIndex.FieldName == row[CommonStrings.Name].ToString()))
                        {
                            attachmentIndex = (DMS_ArchivedDocSearchIndex)var.Single(f => f.DMS_SearchFieldIndex.FieldName == row[CommonStrings.Name].ToString());

                            if (attachmentIndex != null && var.Any(f => f.DMS_SearchFieldIndex.FieldName == row[CommonStrings.Name].ToString()))
                            {
                                ((FieldData)row[CommonStrings.FieldData]).StringValue = attachmentIndex.DMS_SearchFieldIndex.FieldValue;
                                row[CommonStrings.FormattedValue] = ((FieldData)row[CommonStrings.FieldData]).FormattedValue;
                            }

                        }
                        else
                            attachmentInfo.BookmarksDT.Rows.Remove(row);
                    }

                    //tag field value
                    if (var.Any(f => f.DMS_SearchFieldIndex.FieldName == CommonStrings.Tags))
                    {
                        attachmentIndex = (DMS_ArchivedDocSearchIndex)var.Single(f => f.DMS_SearchFieldIndex.FieldName == CommonStrings.Tags);
                        attachmentInfo.Tags = attachmentIndex.DMS_SearchFieldIndex.FieldValue;
                    }
                }
                else
                    attachmentInfo.BookmarksDT.Clear();
            }
            catch (Exception e)
            {
                throw (e);
            }
        }

        //-----------------------------------------------------------------------------------------------
        private void GetAttachmentBookmarksValues(ref AttachmentInfo attachmentInfo)
        {
            try
            {
                //dc.Refresh(RefreshMode.OverwriteCurrentValues, dc.DMS_AttachmentSearchIndexes);

                int attachmentID = attachmentInfo.AttachmentId;
                var var = from attIndexes in dc.DMS_AttachmentSearchIndexes
                          where attIndexes.AttachmentID == attachmentID
                          select attIndexes;

                DataRow row = null;
                if (var != null && var.Any())
                {
                    DMS_AttachmentSearchIndex attachmentIndex = null;

                    for (int i = attachmentInfo.BookmarksDT.Rows.Count - 1; i >= 0; i--)
                    {
                        row = attachmentInfo.BookmarksDT.Rows[i];
                        if (row.RowState == DataRowState.Deleted)
                            continue;

                        if (var.Any(f => f.DMS_SearchFieldIndex.FieldName == row[CommonStrings.Name].ToString()))
                        {
                            attachmentIndex = (DMS_AttachmentSearchIndex)var.Single(f => f.DMS_SearchFieldIndex.FieldName == row[CommonStrings.Name].ToString());

                            if (attachmentIndex != null && var.Any(f => f.DMS_SearchFieldIndex.FieldName == row[CommonStrings.Name].ToString()))
                            {
                                ((FieldData)row[CommonStrings.FieldData]).StringValue = attachmentIndex.DMS_SearchFieldIndex.FieldValue;
                                row[CommonStrings.FormattedValue] = ((FieldData)row[CommonStrings.FieldData]).FormattedValue;
                            }
                        }
                        else
                            attachmentInfo.BookmarksDT.Rows.Remove(row);
                    }

                    //tag field value
                    if (var.Any(f => f.DMS_SearchFieldIndex.FieldName == CommonStrings.Tags))
                    {
                        attachmentIndex = (DMS_AttachmentSearchIndex)var.Single(f => f.DMS_SearchFieldIndex.FieldName == CommonStrings.Tags);
                        attachmentInfo.Tags = attachmentIndex.DMS_SearchFieldIndex.FieldValue;
                    }
                }
                else //nel remoto caso che l'attachment non abbia alcun bookmark allora mantengo i bookmark di default
                    UpdateAttachmentIndexes(attachmentInfo, attachmentInfo.Tags, attachmentInfo.Description);


            }
            catch (Exception e)
            {
                throw (e);
            }
        }

        /// <summary>
        /// dato l'elenco dei campi indicizzati di una coppia documento/attachment assegna ad ogni singolo campo il valore 
        /// letto dalla tabella DMS_SearchFieldIndexes. 
        /// Se il campo non è stato indicizzato per quel documento viene tolto dal CollectionFieldsDataTable 
        /// </summary>
        //-----------------------------------------------------------------------------------------------
        public void GetBookmarksValues(ref AttachmentInfo attachmentInfo)
        {
            try
            {
                if (attachmentInfo.AttachmentId > -1)
                    GetAttachmentBookmarksValues(ref attachmentInfo);
                else
                    GetArchivedDocBookmarksValues(ref attachmentInfo);
            }
            catch (Exception e)
            {
                SetMessage(Strings.ErrorSearchingFieldValues, e, "GetBookmarksValues");
            }
        }



        /// <summary>
        /// grouping the search result byTBNamespace, TBPrimaryKey
        /// </summary>
        /// <param name="resultDataTable"></param>
        //---------------------------------------------------------------------------------
        private void GroupResult(SearchResultDataTable resultDataTable)
        {
            // The attachments are grouped for ERP document
            Dictionary<string, AttachmentsForErpDocument> searchResultMap = new Dictionary<string, AttachmentsForErpDocument>();

            try
            {
                //first I order the result by collector, collection, documentnamespace, documentkey, attachment date
                //EnumerableRowCollection<DataRow> enumDataTable = resultDataTable.AsEnumerable();
                var ordered = from resRow in resultDataTable.AsEnumerable()
                              orderby resRow[CommonStrings.Collector], resRow[CommonStrings.Collection],
                              resRow[CommonStrings.DocNamespace], resRow[CommonStrings.TBPrimaryKey], resRow[CommonStrings.AttachmentDate]
                              select resRow;

                SearchResultDataTable orderedResultDT = new SearchResultDataTable();
                ordered.CopyToDataTable(orderedResultDT, LoadOption.OverwriteChanges);

                //finally I group the result by TBDocumentNamespace+documentKey. 
                //So each doc+key has the list of own attachement document. 
                AttachmentsForErpDocument resDt = null;

                string currKey = string.Empty;
                string oldKey = string.Empty;
                string currTBDocNamespace = string.Empty;
                string currTBPrimaryKey = string.Empty;
                string currDocKeyDescription = string.Empty;

                foreach (DataRow resRow in orderedResultDT.Rows)
                {
                    currKey = string.Format("{0}{1}", resRow[CommonStrings.DocNamespace], resRow[CommonStrings.TBPrimaryKey]);
                    if (string.Compare(currKey, oldKey, StringComparison.InvariantCultureIgnoreCase) != 0)
                    {
                        //when I complete the resultDT for each document/primarykey couple I send the notification
                        if (resDt != null && resDt.Rows.Count > 0 && SearchResultAddingDataTable != null)
                        {
                            //security integration
                            if (!DMSOrchestrator.SecurityEnabled || CheckSecurityOnDocNamespace(currTBDocNamespace))
                            {
                                SearchResultAddingDataTableEventArgs arg = new SearchResultAddingDataTableEventArgs();
                                arg.ResultDataTable = resDt;
                                arg.TBDocNamespace = currTBDocNamespace;
                                arg.TBPrimaryKey = currTBPrimaryKey;
                                arg.DocKeyDescription = currDocKeyDescription;
                                SearchResultAddingDataTable(this, arg);
                            }
                        }
                        oldKey = currKey;
                        currTBDocNamespace = resRow[CommonStrings.DocNamespace].ToString();
                        currTBPrimaryKey = resRow[CommonStrings.TBPrimaryKey].ToString();
                        currDocKeyDescription = GetDocumentKeyDescription((int)resRow[CommonStrings.AttachmentID]);
                    }

                    if (!searchResultMap.TryGetValue(currKey, out resDt))
                    {
                        resDt = new AttachmentsForErpDocument();
                        searchResultMap.Add(currKey, resDt);
                    }

                    DataRow newRow = resDt.NewRow();
                    newRow[CommonStrings.ArchivedDocID] = resRow[CommonStrings.ArchivedDocID];
                    newRow[CommonStrings.AttachmentID] = resRow[CommonStrings.AttachmentID];
                    newRow[CommonStrings.Name] = resRow[CommonStrings.Name];
                    newRow[CommonStrings.AttachmentDate] = resRow[CommonStrings.AttachmentDate];
                    newRow[CommonStrings.ExtensionType] = resRow[CommonStrings.ExtensionType];
                    newRow[CommonStrings.AttachmentInfo] = resRow[CommonStrings.AttachmentInfo];
                    resDt.Rows.Add(newRow);
                }

                // added the last one
                if (resDt != null && resDt.Rows.Count > 0 && SearchResultAddingDataTable != null)
                {
                    //security integration
                    if (!DMSOrchestrator.SecurityEnabled || CheckSecurityOnDocNamespace(currTBDocNamespace))
                    {
                        SearchResultAddingDataTableEventArgs arg = new SearchResultAddingDataTableEventArgs();
                        arg.ResultDataTable = resDt;
                        arg.TBDocNamespace = currTBDocNamespace;
                        arg.TBPrimaryKey = currTBPrimaryKey;
                        arg.DocKeyDescription = currDocKeyDescription;
                        arg.ResultDataTable = resDt;
                        SearchResultAddingDataTable(this, arg);
                    }
                }
            }
            catch (Exception e)
            {
                SetMessage(Strings.ErrorGroupingSearchResults, e, "GroupResult");
            }
        }

        //--------------------------------------------------------------------------------------------
        public List<string> GetBookmarkFieldsToObserve(int collectionID)
        {
            List<string> fieldsToObserve = new List<string>();

            try
            {
                var fields = from cf in dc.DMS_CollectionsFields
                             where cf.CollectionID == collectionID && (cf.FieldGroup == (int?)FieldGroup.Binding || cf.FieldGroup == (int?)FieldGroup.Key)
                             select cf;

                if (fields != null && fields.Any())
                {
                    IEnumerable<DMS_CollectionsField> result = (fields.Select(a => a).AsEnumerable()).Distinct(new DistinctFieldNameCollectionsField());
                    foreach (DMS_CollectionsField field in result)
                        fieldsToObserve.Add(field.FieldName);
                }
            }
            catch (Exception e)
            {
                SetMessage("Error:", e, "GetBookmarkFieldsToObserve");
            }

            return fieldsToObserve;
        }

        //--------------------------------------------------------------------------------------------
        public bool CheckNamespaceAndPrimaryKey(ref string tbDocNamespace, string docPrimaryKey, string guid)
        {
            if (String.IsNullOrWhiteSpace(tbDocNamespace) || (String.IsNullOrWhiteSpace(docPrimaryKey) && String.IsNullOrWhiteSpace(guid)))
				return false;

            if (!tbDocNamespace.StartsWith("Document.", true, System.Globalization.CultureInfo.CurrentCulture))
                tbDocNamespace = "Document." + tbDocNamespace;

            return true;
        }


        /// <summary>
        /// Ritorna tutti gli AttachmentInfo legati ad uno specifico documento di ERP
        /// </summary>
        //--------------------------------------------------------------------------------------------
        public List<AttachmentInfo> GetAttachments(string tbDocNamespace, string tBPrimaryKey, AttachmentFilterType filterType)
        {
            List<AttachmentInfo> searchResult = new List<AttachmentInfo>();
            DMSModelDataContext localdc = null;
            try
            {
                if (!CheckNamespaceAndPrimaryKey(ref tbDocNamespace, tBPrimaryKey, string.Empty))
                    return searchResult;
                
                localdc = new DMSModelDataContext(DMSOrchestrator.DMSConnectionString);

				int max = DMSOrchestrator.SettingsManager.UsersSettingState.Options.AttachmentPanelTempOptionsState.MaxDocNumber;
				int res = 0;

				//Posso estrarre: solo allegati, solo papery oppure entrambi
				//se necessario inzio ad estrarre gli allegati dalla tabella DMS_Attachment
				if (filterType != AttachmentFilterType.OnlyPapery)
                {
                    IQueryable<DMS_Attachment> qAttachment =
                        from att in localdc.DMS_Attachments
                        orderby att.TBModified descending
                        where att.DMS_ErpDocument.DocNamespace == tbDocNamespace && att.DMS_ErpDocument.PrimaryKeyValue == tBPrimaryKey
                        select att;

					if (max > 0)
						qAttachment = qAttachment.Take(max);

					if (filterType == AttachmentFilterType.OnlyMainDoc)
                        qAttachment = qAttachment.Where(a => a.IsMainDoc == true);

					if (filterType == AttachmentFilterType.OnlyForMail)
						qAttachment = qAttachment.Where(a => a.IsForMail == true);

					//anche per gli attachment seguo l'impostazione dei settings
					if (DMSOrchestrator.SettingsManager.UsersSettingState.Options.RepositoryOptionsState.ShowOnlyMyArchivedDocs && !DMSOrchestrator.IsAdmin)
                        qAttachment = qAttachment.Where(a => (a.TBModifiedID == DMSOrchestrator.WorkerId || a.TBCreatedID == DMSOrchestrator.WorkerId));

					//date filter 
					if (
						DMSOrchestrator.SettingsManager.UsersSettingState.Options.AttachmentPanelTempOptionsState.SearchDateRange.StartDate != DateTime.MinValue && 
						DMSOrchestrator.SettingsManager.UsersSettingState.Options.AttachmentPanelTempOptionsState.SearchDateRange.EndDate != DateTime.MaxValue
						)
						qAttachment = qAttachment.Where
							(
							a => (a.TBModified >= DMSOrchestrator.SettingsManager.UsersSettingState.Options.AttachmentPanelTempOptionsState.SearchDateRange.StartDate && 
								a.TBModified <= DMSOrchestrator.SettingsManager.UsersSettingState.Options.AttachmentPanelTempOptionsState.SearchDateRange.EndDate) ||
								(a.TBCreated >= DMSOrchestrator.SettingsManager.UsersSettingState.Options.AttachmentPanelTempOptionsState.SearchDateRange.StartDate && 
								a.TBCreated <= DMSOrchestrator.SettingsManager.UsersSettingState.Options.AttachmentPanelTempOptionsState.SearchDateRange.EndDate)
							);

					foreach (DMS_Attachment attachment in qAttachment)
                    {
                        AttachmentInfo newAttInfo = new AttachmentInfo(attachment, DMSOrchestrator);
                        searchResult.Add(newAttInfo);
                    }

					res = qAttachment.Count();
				}

				//se ho un max di documenti da filtrare e li ho gia presi con la query sopra salto i papery
				if (max > 0 && max == res)
					return searchResult;

				//se devo estrarre solo i cartacei oppure sia allegati che cartacei allora effettuo la query sulla tabella DMS_ErpDocBarcodes
				if (filterType == AttachmentFilterType.OnlyPapery || filterType == AttachmentFilterType.Both)
                {
                    IQueryable<DMS_ErpDocBarcode> qErpDocBarcodes =
                        from erpDoc in dc.DMS_ErpDocBarcodes
                        where erpDoc.DMS_ErpDocument.DocNamespace == tbDocNamespace &&
                        erpDoc.DMS_ErpDocument.PrimaryKeyValue == tBPrimaryKey
                        select erpDoc;

					//se ho un numero di doc da filtrare prendo solo quelli che mi mancano per arrivare al numero massimo tolti gli attachment già presi
					if (max > 0)
						qErpDocBarcodes = qErpDocBarcodes.Take(max - res);

					foreach (DMS_ErpDocBarcode edb in qErpDocBarcodes)
                    {
                        AttachmentInfo newAttInfo = new AttachmentInfo(edb, DMSOrchestrator);
                        searchResult.Add(newAttInfo);
                    }
                }
            }
            catch (Exception e)
            {
                SetMessage(Strings.ErrorLoadingAttachments, e, "GetAttachments");
            }
            finally
            {
                if (localdc != null)
                    localdc.Dispose();
            }

            return searchResult;
        }

		/// <summary>
		/// Ritorna tutti gli AttachmentInfo legati ad uno specifico documento di ERP
		/// </summary>
		//--------------------------------------------------------------------------------------------
		public List<AttachmentInfo> GetAttachmentsByGuid(string tbDocNamespace, string docGuid, AttachmentFilterType filterType)
		{
			List<AttachmentInfo> searchResult = new List<AttachmentInfo>();
			DMSModelDataContext localdc = null;
			try
			{
				if (!CheckNamespaceAndPrimaryKey(ref tbDocNamespace, string.Empty, docGuid))
					return searchResult;

				Guid tbGuid = Guid.Parse(docGuid);		
				localdc = new DMSModelDataContext(DMSOrchestrator.DMSConnectionString);

				int max = DMSOrchestrator.SettingsManager.UsersSettingState.Options.AttachmentPanelTempOptionsState.MaxDocNumber;
				int res = 0;

				//Posso estrarre: solo allegati, solo papery oppure entrambi
				//se necessario inzio ad estrarre gli allegati dalla tabella DMS_Attachment
				if (filterType != AttachmentFilterType.OnlyPapery)
				{
					IQueryable<DMS_Attachment> qAttachment =
						from att in localdc.DMS_Attachments
						orderby att.TBModified descending
						where att.DMS_ErpDocument.DocNamespace == tbDocNamespace && att.DMS_ErpDocument.TBGuid == tbGuid
						select att;

					if (max > 0)
						qAttachment = qAttachment.Take(max);

					if (filterType == AttachmentFilterType.OnlyMainDoc)
						qAttachment = qAttachment.Where(a => a.IsMainDoc == true);

					if (filterType == AttachmentFilterType.OnlyForMail)
						qAttachment = qAttachment.Where(a => a.IsForMail == true);

					//anche per gli attachment seguo l'impostazione dei settings
					if (DMSOrchestrator.SettingsManager.UsersSettingState.Options.RepositoryOptionsState.ShowOnlyMyArchivedDocs && !DMSOrchestrator.IsAdmin)
						qAttachment = qAttachment.Where(a => (a.TBModifiedID == DMSOrchestrator.WorkerId || a.TBCreatedID == DMSOrchestrator.WorkerId));

					//date filter 
					if (
						DMSOrchestrator.SettingsManager.UsersSettingState.Options.AttachmentPanelTempOptionsState.SearchDateRange.StartDate != DateTime.MinValue &&
						DMSOrchestrator.SettingsManager.UsersSettingState.Options.AttachmentPanelTempOptionsState.SearchDateRange.EndDate != DateTime.MaxValue
						)
						qAttachment = qAttachment.Where
							(
							a => (a.TBModified >= DMSOrchestrator.SettingsManager.UsersSettingState.Options.AttachmentPanelTempOptionsState.SearchDateRange.StartDate &&
								a.TBModified <= DMSOrchestrator.SettingsManager.UsersSettingState.Options.AttachmentPanelTempOptionsState.SearchDateRange.EndDate) ||
								(a.TBCreated >= DMSOrchestrator.SettingsManager.UsersSettingState.Options.AttachmentPanelTempOptionsState.SearchDateRange.StartDate &&
								a.TBCreated <= DMSOrchestrator.SettingsManager.UsersSettingState.Options.AttachmentPanelTempOptionsState.SearchDateRange.EndDate)
							);

					foreach (DMS_Attachment attachment in qAttachment)
					{
						AttachmentInfo newAttInfo = new AttachmentInfo(attachment, DMSOrchestrator);
						searchResult.Add(newAttInfo);
					}

					res = qAttachment.Count();
				}

				//se ho un max di documenti da filtrare e li ho gia presi con la query sopra salto i papery
				if (max > 0 && max == res)
					return searchResult;

				//se devo estrarre solo i cartacei oppure sia allegati che cartacei allora effettuo la query sulla tabella DMS_ErpDocBarcodes
				if (filterType == AttachmentFilterType.OnlyPapery || filterType == AttachmentFilterType.Both)
				{
					IQueryable<DMS_ErpDocBarcode> qErpDocBarcodes =
						from erpDoc in dc.DMS_ErpDocBarcodes
						where erpDoc.DMS_ErpDocument.DocNamespace == tbDocNamespace &&
						erpDoc.DMS_ErpDocument.TBGuid == tbGuid
						select erpDoc;

					//se ho un numero di doc da filtrare prendo solo quelli che mi mancano per arrivare al numero massimo tolti gli attachment già presi
					if (max > 0)
						qErpDocBarcodes = qErpDocBarcodes.Take(max - res);

					foreach (DMS_ErpDocBarcode edb in qErpDocBarcodes)
					{
						AttachmentInfo newAttInfo = new AttachmentInfo(edb, DMSOrchestrator);
						searchResult.Add(newAttInfo);
					}
				}
			}
			catch (Exception e)
			{
				SetMessage(Strings.ErrorLoadingAttachments, e, "GetAttachmentsByGuid");
			}
			finally
			{
				if (localdc != null)
					localdc.Dispose();
			}

			return searchResult;
		}


		//--------------------------------------------------------------------------------
		public int GetErpDocumentID(string tbDocNamespace, string tbPrimaryKey)
        {
            IQueryable<int> erpDocumentID = (from erpDoc in dc.DMS_ErpDocuments
                                             where erpDoc.DocNamespace == tbDocNamespace && erpDoc.PrimaryKeyValue == tbPrimaryKey
                                             select erpDoc.ErpDocumentID);

            if (erpDocumentID == null || !erpDocumentID.Any())
                return -1;

            return erpDocumentID.Single();
        }


        //fornisce il numero di allegati, papery, allegati+papery relativi ad un documento di mago a seconda del valore di filterType
        //uso una sqlconnection per performance
        //--------------------------------------------------------------------------------
        public int GetAttachmentsCount(string tbDocNamespace, string tbPrimaryKey, AttachmentFilterType filterType)
        {
            if (!CheckNamespaceAndPrimaryKey(ref tbDocNamespace, tbPrimaryKey, string.Empty))
                return 0;

            return GetAttachmentsCount(GetErpDocumentID(tbDocNamespace, tbPrimaryKey), filterType);
        }

        //--------------------------------------------------------------------------------
        public int GetAttachmentsCount(int erpDocumentID, AttachmentFilterType filterType)
        {
            int attCount = 0;
            int paperyCount = 0;

            if (erpDocumentID < 0)
                return 0;

            if (filterType != AttachmentFilterType.OnlyPapery)
            //prima conteggio gli allegati 
            {
                if (DMSOrchestrator.SettingsManager.UsersSettingState.Options.RepositoryOptionsState.ShowOnlyMyArchivedDocs && !DMSOrchestrator.IsAdmin)
                    attCount = (from att in dc.DMS_Attachments where att.ErpDocumentID == erpDocumentID && (att.TBModifiedID == DMSOrchestrator.WorkerId || att.TBCreatedID == DMSOrchestrator.WorkerId) select att).Count();
                else
                    attCount = (from att in dc.DMS_Attachments where att.ErpDocumentID == erpDocumentID select att).Count();
            }

            //poi conteggio gli eventuali papery
            if (filterType == AttachmentFilterType.OnlyPapery || filterType == AttachmentFilterType.Both)
                paperyCount = (from pap in dc.DMS_ErpDocBarcodes where pap.ErpDocumentID == erpDocumentID select pap).Count();


            return attCount + paperyCount;
        }

        //Security integration
        //--------------------------------------------------------------------------------
        public ERPDocumentsGrant LoadERPDocumentsGrant()
        {
            //carico i namespace dei documenti che contengono allegati
            if (erpDocumentsGrant == null)
                erpDocumentsGrant = new ERPDocumentsGrant();
            else
                erpDocumentsGrant.Clear();

            dc.Refresh(RefreshMode.OverwriteCurrentValues, dc.DMS_ErpDocuments);

            IEnumerable<string> documents = (dc.DMS_ErpDocuments.Select(d => d.DocNamespace).AsEnumerable()).Distinct(new DistinctDocNamespace());

            if (documents != null && documents.Any())
            {
                foreach (string doc in documents)
                {
                    DataRow row = erpDocumentsGrant.NewRow();
                    row[CommonStrings.DocNamespace] = doc;
                    row[CommonStrings.CanShowAttachments] = CUtility.CanUseDataEntryNamespace(doc);
                    erpDocumentsGrant.Rows.Add(row);
                }
            }
            return erpDocumentsGrant;
        }

        //--------------------------------------------------------------------------------
        private bool CheckSecurityOnDocNamespace(string docNamespace)
        {
            if (erpDocumentsGrant == null)
                LoadERPDocumentsGrant();
            DataRow erpDoc = erpDocumentsGrant.Rows.Find(docNamespace);
            if (erpDoc == null)
            {
                erpDoc = erpDocumentsGrant.NewRow();
                erpDoc[CommonStrings.DocNamespace] = docNamespace;
                erpDoc[CommonStrings.CanShowAttachments] = CUtility.CanUseDataEntryNamespace(docNamespace);
                erpDocumentsGrant.Rows.Add(erpDoc);
            }
            return (bool)erpDoc[CommonStrings.CanShowAttachments];
        }

        //--------------------------------------------------------------------------------
        private bool CheckSecurityOnArchivedDoc(int archiveDocId)
        {
            if (erpDocumentsGrant == null)
                LoadERPDocumentsGrant();

            //carico tutti gli allegati che fanno riferimento a archiveDocId facendone la distinct per namespace di document
            IEnumerable<string> namespaces =
                (dc.DMS_Attachments.Where(d => d.ArchivedDocID == archiveDocId).Select(d => d.DMS_ErpDocument.DocNamespace)).AsEnumerable().Distinct(new DistinctDocNamespace());

            if (namespaces == null || !namespaces.Any())
                return true;

            bool grant = false;

            foreach (string docNamespace in namespaces)
                grant = CheckSecurityOnDocNamespace(docNamespace) || grant;

            return grant;
        }

        //--------------------------------------------------------------------------------
        public ArchivedDocDataTable FillArchiveDocDT(IEnumerable<int> archivedDocList)
        {
            ArchivedDocDataTable searchResult = new ArchivedDocDataTable();
            if (archivedDocList == null || archivedDocList.Count() <= 0)
                return searchResult;

            try
            {
                //security integration
                if (DMSOrchestrator.SecurityEnabled)
                    LoadERPDocumentsGrant();

                foreach (int docId in archivedDocList)
                {
                    var docs = from d in dc.DMS_ArchivedDocuments where d.ArchivedDocID == docId select d;
                    if (docs == null || !docs.Any())
                        continue;
                    DMS_ArchivedDocument ar = docs.Single();

                    //security integration
                    if (DMSOrchestrator.SecurityEnabled && !CheckSecurityOnArchivedDoc(ar.ArchivedDocID))
                        continue;

                    DataRow newRow = searchResult.NewRow();
                    newRow[CommonStrings.ArchivedDocID] = ar.ArchivedDocID;
                    newRow[CommonStrings.Name] = ar.Name;
                    newRow[CommonStrings.Description] = ar.Description;
                    newRow[CommonStrings.TBCreated] = ar.TBCreated;
                    newRow[CommonStrings.TBModified] = ar.TBModified;
                    newRow[CommonStrings.ModifierID] = (ar.ModifierID != null) ? (int)ar.ModifierID : -1;
                    newRow[CommonStrings.WorkerName] = DMSOrchestrator.GetWorkerName(ar.TBModifiedID);
                    newRow[CommonStrings.StorageType] = (ar.StorageType != null) ? (int)ar.StorageType : 0;
                    newRow[CommonStrings.IsWoormReport] = ar.IsWoormReport;

                    var attDoc = (from att in dc.DMS_Attachments
                                  where att.ArchivedDocID == ar.ArchivedDocID
                                  select att.AttachmentID).Take(1);

                    newRow[CommonStrings.Attached] = (attDoc != null && attDoc.Any());

                    searchResult.Rows.Add(newRow);
                    newRow[CommonStrings.AttachmentInfo] = new AttachmentInfo(ar, DMSOrchestrator);
                }
            }
            catch (Exception e)
            {
                throw (e);
            }

            return searchResult;
        }

        //--------------------------------------------------------------------------------
        public ArchivedDocDataTable FillArchiveDocDT(IEnumerable<DMS_ArchivedDocument> baseResult)
        {
            ArchivedDocDataTable searchResult = new ArchivedDocDataTable();

            try
            {
                //security integration
                if (DMSOrchestrator.SecurityEnabled)
                    LoadERPDocumentsGrant();

                foreach (DMS_ArchivedDocument doc in baseResult)
                {
                    //security integration
                    if (DMSOrchestrator.SecurityEnabled && !CheckSecurityOnArchivedDoc(doc.ArchivedDocID))
                        continue;

                    DataRow newRow = searchResult.NewRow();
                    newRow[CommonStrings.ArchivedDocID] = doc.ArchivedDocID;
                    newRow[CommonStrings.Name] = doc.Name;
                    newRow[CommonStrings.Description] = doc.Description;
                    newRow[CommonStrings.TBCreated] = doc.TBCreated;
                    newRow[CommonStrings.TBModified] = doc.TBModified;
                    newRow[CommonStrings.ModifierID] = (doc.ModifierID != null) ? (int)doc.ModifierID : -1;
                    MWorker worker = DMSOrchestrator.WorkersTable.GetWorker((int)doc.TBModifiedID);
                    newRow[CommonStrings.WorkerName] = (worker != null) ? worker.Name + " " + worker.LastName : doc.TBModifiedID.ToString();
                    newRow[CommonStrings.StorageType] = (doc.StorageType != null) ? (int)doc.StorageType : 0;
                    newRow[CommonStrings.IsWoormReport] = doc.IsWoormReport;

                    var attDoc = (from att in dc.DMS_Attachments
                                  where att.ArchivedDocID == doc.ArchivedDocID
                                  select att.AttachmentID).Take(1);

                    newRow[CommonStrings.Attached] = (attDoc != null && attDoc.Any());
                    searchResult.Rows.Add(newRow);
                    newRow[CommonStrings.AttachmentInfo] = new AttachmentInfo(doc, DMSOrchestrator);
                }
            }
            catch (Exception e)
            {
                throw (e);
            }

            return searchResult;
        }


		//--------------------------------------------------------------------------------
		internal ArchivedDocDataTable GetArchivedDocuments()
		{

			ArchivedDocDataTable searchResult = new ArchivedDocDataTable();

			SqlConnection connection = null;
			SqlCommand command = null;
			SqlDataReader reader = null;
			try
			{
				connection = new SqlConnection(DMSOrchestrator.DMSConnectionString);
				connection.Open();
				command = new SqlCommand();
				command.Connection = connection;

				string docNamespaces = string.Empty;
				if (DMSOrchestrator.SecurityEnabled)
				{
					ERPDocumentsGrant docsGrant = LoadERPDocumentsGrant();
					foreach (DataRow docNamespace in docsGrant.Rows)
					{
						if (!(bool)docNamespace[CommonStrings.CanShowAttachments])
						{
							if (!string.IsNullOrEmpty(docNamespaces))
								docNamespaces += ", ";
							docNamespaces += string.Format(@"'{0}'", docNamespace[CommonStrings.DocNamespace]);
						}
					}
				}
				bool useSecurity = DMSOrchestrator.SecurityEnabled && !string.IsNullOrEmpty(docNamespaces);

				//qui estraggo i documenti con o senza allegati senza effettuare alcun filtro di security se previsto dai parametri viene fatto il filtro sull'utente che ha archiviato il documento
				string cmdText = "Select DISTINCT  D.ArchivedDocID, D.Name, D.TBModified, D.TBModifiedID, D.ModifierID, D.StorageType, count(A.ArchivedDocID) as attCount from DMS_ArchivedDocument D LEFT OUTER JOIN DMS_Attachment A ON D.ArchivedDocID = A.ArchivedDocID";
				if (DMSOrchestrator.SettingsManager.UsersSettingState.Options.RepositoryOptionsState.ShowOnlyMyArchivedDocs && !DMSOrchestrator.IsAdmin)
					cmdText += string.Format(" WHERE (D.TBCreatedID = {0} OR D.TBModifiedID = {0}) ", DMSOrchestrator.WorkerId.ToString());
				cmdText += " GROUP BY D.ArchivedDocID, D.Name, D.TBModified, D.TBModifiedID, D.ModifierID, D.StorageType";

				if (useSecurity)
				{
					cmdText += " HAVING count(A.ArchivedDocID) = 0"; //estraggo i soli documenti senza allegati visto che per gli allegati devo controllare la security. Questo viene fatto nella query di union
																	 //se è prevista la security allora devo effetuare anche la select dei soli documenti archiviati che hanno allegati che soddisfano i criteri di sicurity
					cmdText += " UNION ";
					cmdText += "Select DISTINCT  D.ArchivedDocID, D.Name, D.TBModified, D.TBModifiedID, D.ModifierID, D.StorageType, count(A.ArchivedDocID) as attCount from DMS_ArchivedDocument D, DMS_Attachment A, DMS_ErpDocument E WHERE ";
					if (DMSOrchestrator.SettingsManager.UsersSettingState.Options.RepositoryOptionsState.ShowOnlyMyArchivedDocs && !DMSOrchestrator.IsAdmin)
						cmdText += string.Format("(D.TBCreatedID = {0} OR D.TBModifiedID = {0}) AND ", DMSOrchestrator.WorkerId.ToString());
					cmdText += string.Format("D.ArchivedDocID = A.ArchivedDocID AND (A.ErpDocumentID = E.ErpDocumentID  AND A.ErpDocumentID = E.ErpDocumentID AND E.DocNamespace NOT IN ({0}))", docNamespaces);
					cmdText += " GROUP BY D.ArchivedDocID, D.Name, D.TBModified, D.TBModifiedID, D.ModifierID, StorageType";

					command.CommandText = "SELECT Distinct * FROM ( " + cmdText + " ) AS ResultTable ORDER BY TBModified DESC";
				}
				else
					command.CommandText = cmdText + " ORDER BY D.TBModified DESC";

				reader = command.ExecuteReader();

				while (reader.Read())
				{
					DataRow newRow = searchResult.NewRow();
					newRow[CommonStrings.ArchivedDocID] = reader[CommonStrings.ArchivedDocID];
					newRow[CommonStrings.Name] = reader[CommonStrings.Name];
					newRow[CommonStrings.TBModified] = reader[CommonStrings.TBModified];

					MWorker worker = DMSOrchestrator.WorkersTable.GetWorker((int)reader[CommonStrings.TBModifiedID]);
					newRow[CommonStrings.WorkerName] = (worker != null) ? worker.Name + " " + worker.LastName : reader[CommonStrings.TBModifiedID].ToString();
					newRow[CommonStrings.ModifierID] = reader[CommonStrings.ModifierID];
					newRow[CommonStrings.Attached] = (int)reader["attCount"] > 0;
					newRow[CommonStrings.StorageType] = (reader.IsDBNull(reader.GetOrdinal(CommonStrings.StorageType))) ? 0 : (int)reader["StorageType"];

					//@@TODOMICHI: aggiungere anche questi campi in questa query?
					//newRow[CommonStrings.TBCreated] = reader[CommonStrings.TBCreated];
					//newRow[CommonStrings.Description] = reader[CommonStrings.Description].ToString();
					//newRow[CommonStrings.IsWoormReport] = reader[CommonStrings.IsWoormReport].ToString();

					searchResult.Rows.Add(newRow);
					newRow[CommonStrings.AttachmentInfo] = null; //on demand. Lo carico solo se necessario
				}
			}
			catch (SqlException e)
			{
				SetMessage(Strings.ErrorLoadingArchivedDoc, e, "GetArchivedDocuments");
			}
			finally
			{
				if (reader != null)
					reader.Close();

				if (command != null)
					command.Dispose();

				if (connection != null)
				{
					if (connection.State == ConnectionState.Open)
						connection.Close();
					connection.Dispose();
				}
			}

			return searchResult;
		}

		//--------------------------------------------------------------------------------
		public AttachmentInfo GetAttachmentInfoFromArchivedDocId(int archivedDocId)
        {
            AttachmentInfo attInfo = null;
            try
            {
                var archDoc = from archived in dc.DMS_ArchivedDocuments
                              where archived.ArchivedDocID == archivedDocId
                              select archived;

                if (archDoc != null && archDoc.Any())
                    attInfo = new AttachmentInfo(archDoc.Single(), DMSOrchestrator);
            }
            catch (Exception e)
            {
                SetMessage(Strings.ErrorLoadingArchivedDoc, e, "GetAttachmentInfoFromArchivedDocId");
            }

            return attInfo;
        }


        //--------------------------------------------------------------------------------
        public AttachmentInfo GetAttachmentInfoFromAttachmentId(int attachmentId)
        {
            AttachmentInfo attInfo = null;
            try
            {
                var attachment = from attachments in dc.DMS_Attachments
                                 where attachments.AttachmentID == attachmentId
                                 select attachments;

                if (attachment != null && attachment.Any())
                    attInfo = new AttachmentInfo(attachment.Single(), DMSOrchestrator);
            }
            catch (Exception e)
            {
                SetMessage(Strings.ErrorLoadingArchivedDoc, e, "GetAttachmentInfoFromAttachmentId");
            }

            return attInfo;
        }


        //---------------------------------------------------------------------
        internal List<int> GetAttachmentsForArchivedDocId(int archivedDocId)
        {
            List<int> attachmentList = new List<int>();

            var attachments = (from att in dc.DMS_Attachments
                               where att.ArchivedDocID == archivedDocId
                               select att.AttachmentID);

            if (attachments != null && attachments.Any())
                attachmentList = attachments.ToList();

            return attachmentList;
        }


        //chiamato dal mondo managed per eseguire i filtri 
        //--------------------------------------------------------------------------------
        internal ArchivedDocDataTable GetArchivedDocuments(FilterEventArgs fea)
        {
            try
            {
                this.fea = fea;
                return GetSqlArchivedDocuments();
            }
            catch (Exception e)
            {
                SetMessage(Strings.ErrorLoadingArchivedDoc, e, "GetArchivedDocuments");
                return null;
            }
        }


        //---------------------------------------------------------------------
        public string GetStorageFileName(AttachmentInfo attachInfo)
        {
            if (attachInfo.StorageType == StorageTypeEnum.Database)
                return Strings.BinarySaveOnDB;

            string fileStoragePath = string.Empty;
            try
            {
                using (IDbConnection myConnection = new SqlConnection(DMSOrchestrator.DMSConnectionString))
                {
                    myConnection.Open();

                    using (IDbCommand myCommand = myConnection.CreateCommand())
                    {
                        //se è di tipo file system allora nel campo ho il nome del file presente nel repository su file system
                        if (attachInfo.StorageType == StorageTypeEnum.FileSystem)
                        {
                            myCommand.CommandText = string.Format("SELECT StorageFile FROM DMS_ArchivedDocContent WHERE ArchivedDocID = {0}", attachInfo.ArchivedDocId.ToString());
                            fileStoragePath = (string)myCommand.ExecuteScalar();
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                SetMessage(Strings.ErrorLoadingArchivedDoc, e, "GetStorageFileName");
            }

            return fileStoragePath;
        }

        // Il contenuto binario e testuale del documento archiviato viene fatto sulla tabella DMS_ArchivedDocContent
        // che non fa parte del data model questo per evitare di tenersi in memoria il binario 
        // Linq per evitare di fare roundtrip al database si cache in memoria i dati estratti per ogni entity
        // per evitare questo la gestione del binario viene fatta utilizzando le classi del SqlClient leggendolo solo quando strettamente
        // necessario
        //---------------------------------------------------------------------
        public byte[] GetBinaryContent(AttachmentInfo attachInfo, ref bool veryLargeFile)
        {
            byte[] contentArray = null;

            string fileStoragePath = string.Empty;
            try
            {
                using (IDbConnection myConnection = new SqlConnection(DMSOrchestrator.DMSConnectionString))
                {
                    myConnection.Open();

                    using (IDbCommand myCommand = myConnection.CreateCommand())
                    {
                        //se è di tipo file system allora nel campo ho il nome del file presente nel repository su file system
                        if (attachInfo.StorageType == StorageTypeEnum.FileSystem)
                        {
                            myCommand.CommandText = string.Format("SELECT StorageFile FROM DMS_ArchivedDocContent WHERE ArchivedDocID = {0}", attachInfo.ArchivedDocId.ToString());
                            fileStoragePath = (string)myCommand.ExecuteScalar();
                            if (File.Exists(fileStoragePath))
                                contentArray = File.ReadAllBytes(fileStoragePath);
                        }
                        else
                        {
                            myCommand.CommandText = string.Format("SELECT BinaryContent FROM DMS_ArchivedDocContent WHERE ArchivedDocID = {0}", attachInfo.ArchivedDocId.ToString());
                            contentArray = (byte[])myCommand.ExecuteScalar();
                        }

                        if (contentArray == null)
                            SetMessage(Strings.ErrorLoadingArchivedDoc, null, "GetBinaryContent");
                    }
                }
            }
            catch (SqlException e)
            {
                SetMessage(Strings.ErrorLoadingArchivedDoc, e, "GetBinaryContent");
            }
            catch (OutOfMemoryException)
            {
                // nel caso di eccezione OutOfMemoryException tento nuovamente di leggere
                // il documento dividendolo prima in piccoli pezzi
                contentArray = null;

                if (attachInfo.StorageType == StorageTypeEnum.FileSystem)
                {
                    veryLargeFile = true;
                    FileInfo fi = new FileInfo(attachInfo.TempPath);
                    // se il file non esiste oppure la sua data e' diversa da quella del file temporaneo 
                    if (!fi.Exists || (attachInfo.LastWriteTimeUtc > fi.LastWriteTimeUtc))
                    {
                        if (fi.Exists)
                        {
                            bool isReadOnly = fi.IsReadOnly;
                            fi.IsReadOnly = false;
                            File.Copy(fileStoragePath, attachInfo.TempPath);
                            fi.IsReadOnly = isReadOnly;
                        }
                        else
                            File.Copy(fileStoragePath, attachInfo.TempPath);

						fi.Refresh();

						fi.Attributes = fi.Attributes & ~FileAttributes.Hidden;
                        fi.Attributes = fi.Attributes & ~FileAttributes.System;
                    }
                }
                else
                    veryLargeFile = GetBinaryContentForBigFile(attachInfo);
            }

            return contentArray;
        }



        //---------------------------------------------------------------------
        public bool GetBinaryContentForBigFile(AttachmentInfo attachInfo)
        {
            try
            {
                using (IDbConnection connection = new SqlConnection(DMSOrchestrator.DMSConnectionString))
                {
                    connection.Open();

                    using (IDbCommand command = connection.CreateCommand())
                    {
                        command.CommandText = @"SELECT SUBSTRING([BinaryContent], @start, @length) FROM [DMS_ArchivedDocContent] WHERE ArchivedDocID = @DocID";

                        SqlParameter archDocIDParam = new SqlParameter("@DocID", SqlDbType.Int);
                        archDocIDParam.Value = Convert.ToInt32(attachInfo.ArchivedDocId);
                        command.Parameters.Add(archDocIDParam);

                        SqlParameter startParam = new SqlParameter("@start", SqlDbType.BigInt);
                        command.Parameters.Add(startParam);

                        SqlParameter lengthParam = new SqlParameter("@length", SqlDbType.BigInt);
                        command.Parameters.Add(lengthParam);

                        long bytesRead = 0;

                        FileInfo fi = new FileInfo(attachInfo.TempPath);
                        // se il file non esiste oppure la sua data e' diversa da quella del file temporaneo 
                        if (!fi.Exists || (attachInfo.LastWriteTimeUtc > fi.LastWriteTimeUtc))
                        {
                            bool isReadOnly = false;
                            if (fi.Exists)
                            {
                                isReadOnly = fi.IsReadOnly;
                                fi.IsReadOnly = false;
                            }

                            using (FileStream fs = new FileStream(attachInfo.TempPath, FileMode.Create, FileAccess.ReadWrite))
                            {
                                while (bytesRead < attachInfo.OriginalSize)
                                {
                                    startParam.Value = (bytesRead == 0) ? 1 : bytesRead + 1;
                                    // leggiamo a blocchi di 30MB
									lengthParam.Value = (attachInfo.OriginalSize - bytesRead) > 31457280 ? 31457280 : (attachInfo.OriginalSize - bytesRead);
									
									byte[] buffer = (byte[])command.ExecuteScalar();
                                    bytesRead += buffer.Length;

                                    // si e' optato per salvare il content in un file per problemi di outofmemory in caricamento
                                    // di file di dimensioni molto grandi utilizzando i buffer di byte
                                    fs.Write(buffer, 0, buffer.Length);
									buffer = null;
								}
                            }

							fi.Refresh();

                            if (fi.Exists)
                                fi.IsReadOnly = isReadOnly;

                            fi.Attributes = fi.Attributes & ~FileAttributes.Hidden;
                            fi.Attributes = fi.Attributes & ~FileAttributes.System;
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                SetMessage(Strings.ErrorLoadingArchivedDoc, e, "GetBinaryContentForBigFile");
                return false;
            }
            catch (OutOfMemoryException e)
            {
                SetMessage(Strings.ErrorLoadingArchivedDoc, e, "GetBinaryContentForBigFile");
                return false;
            }

            return true;
        }

        //-------------------------------------------------------------------------------------------------
        public static Expression<Func<DMS_AttachmentSearchIndex, bool>> IsCustomFilterCollector(Dictionary<int, IList<int>> customFilters)
        {
            var checkValues = PredicateBuilder.False<DMS_AttachmentSearchIndex>();
            if (customFilters == null)
                return checkValues;

            foreach (KeyValuePair<int, IList<int>> i in customFilters)
                foreach (int ii in i.Value as List<int>)
                {
                    int temp = ii;
                    checkValues = checkValues.Or(p => (p.DMS_Attachment.CollectionID == temp));
                }

            return checkValues;
        }

        /*I create the first part of the search query. Something like this:
			select distinct(AS.AttachmentID) from DMS_AttachmentSearchIndex as AS, DMS_SearchFieldIndex as SF  DMS_Attachment as A, DMS_Collector as C, DMS_Collection as C1, DMS_ErpDocument as E, DMS_ArchivedDocument as AD 
			where AS.SearchFieldID = SF.SearchFieldID AND A.AttachmentID = AS.AttachmentID AND A.CollectionID = C1.CollectionID AND C1.CollectorID = C.CollectorID AND A.ArchivedDocID =  D.ArchivedDocID AND A.ErpDocumentID = E.ErpDocumentID 
			AND C.Name = 'SearchRules.CollectorName' AND C1.Name = "SearchRules.CollectionName" AND E.DocNamespace = "DMSOrchestrator.DocumentNamespace" AND E.PrimaryKey = "DMSOrchestrator.DocumentPrimaryKey"
			AND A.TBCreated >= SearchRules.StartDate AND A.TBCreated <= SearchRules.EndDate  
			AND D.ExtensionType == SearchRules.DocExtensionType			

		  note how with LINQ is more easy.
		*/
        //---------------------------------------------------------------------
        private IQueryable<DMS_AttachmentSearchIndex> CreateBaseSearchQuery()
        {
            IQueryable<DMS_AttachmentSearchIndex> attachments = dc.DMS_AttachmentSearchIndexes;

            try
            {

                switch (SearchRules.SearchContext)
                {
                    case SearchContext.Current:
                        attachments = attachments.Where(att =>
                                     att.DMS_Attachment.DMS_ErpDocument.DocNamespace == SearchRules.DocumentNamespace &&
                                     att.DMS_Attachment.DMS_ErpDocument.PrimaryKeyValue == SearchRules.DocumentPrimaryKey);
                        break;

                    case SearchContext.Collection:
                        attachments = attachments.Where(att => att.DMS_Attachment.DMS_Collection.DMS_Collector.Name == SearchRules.CollectorName &&
                                     att.DMS_Attachment.DMS_Collection.Name == SearchRules.CollectionName);
                        break;

                    case SearchContext.Collector:
                        attachments = attachments.Where(att => att.DMS_Attachment.DMS_Collection.DMS_Collector.Name == SearchRules.CollectorName);
                        break;

                    case SearchContext.Custom:
                        attachments = attachments.Where(IsCustomFilterCollector(SearchRules.CustomFilters));
                        break;
                }


                //date filter
                if (SearchRules.StartDate != DateTime.MinValue && SearchRules.EndDate != DateTime.MaxValue)
                    attachments = attachments.Where(a => (a.DMS_Attachment.TBModified >= SearchRules.StartDate && a.DMS_Attachment.TBModified <= SearchRules.EndDate) ||
                                                 (a.DMS_Attachment.TBCreated >= SearchRules.StartDate && a.DMS_Attachment.TBCreated <= SearchRules.EndDate));

                if (
                        !string.IsNullOrEmpty(SearchRules.DocExtensionType) &&
                        string.Compare(SearchRules.DocExtensionType, CommonStrings.AllExtensions, StringComparison.InvariantCultureIgnoreCase) != 0
                    )
                    attachments = attachments.Where(a => a.DMS_Attachment.DMS_ArchivedDocument.ExtensionType == SearchRules.DocExtensionType);
            }
            catch (Exception e)
            {
                SetMessage(Strings.ErrorCreatingBaseSearchResult, e, "CreateBaseSearchQuery");
            }
            return attachments;
        }

        //--------------------------------------------------------------------------------------------
        public CollectorsResultDataTable GetAllCollectors()
        {
            CollectorsResultDataTable searchResult = new CollectorsResultDataTable();
            try
            {
                var v = from col in dc.DMS_Collectors
                        where col.Name != CommonStrings.EACollector && col.IsStandard == true
                        select col;

                foreach (DMS_Collector coll in v)
                {
                    DataRow newRow = searchResult.NewRow();
                    newRow[CommonStrings.CollectorID] = coll.CollectorID;
                    newRow[CommonStrings.Collector] = coll.Name;
                    newRow[CommonStrings.Collections] = coll.DMS_Collections.ToArray();
                    searchResult.Rows.Add(newRow);
                }
                return searchResult;
            }
            catch (Exception e)
            {
                SetMessage(Strings.ErrorLoadingArchivedDoc, e, "GetAllCollectors");
            }

            return null;
        }

        //--------------------------------------------------------------------------------------------
        public CollectionResultDataTable GetUsedCollections()
        {
            CollectionResultDataTable collectionDT = new CollectionResultDataTable();
            {
                SqlConnection connection = null;
                SqlCommand command = null;
                SqlDataReader reader = null;
                try
                {
                    connection = new SqlConnection(DMSOrchestrator.DMSConnectionString);
                    connection.Open();
                    command = new SqlCommand();
                    command.Connection = connection;
                    command.CommandText = "Select distinct a.CollectionID, c.CollectorID, c.Name, c.IsStandard, e.DocNamespace from DMS_Attachment a, DMS_Collection c, DMS_ErpDocument e where a.CollectionID = c.CollectionID AND a.ErpDocumentID = e.ErpDocumentID";
                    reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        DataRow newRow = collectionDT.NewRow();
                        newRow[CommonStrings.CollectorID] = reader[CommonStrings.CollectorID];
                        newRow[CommonStrings.CollectionID] = reader[CommonStrings.CollectionID];
                        newRow[CommonStrings.Name] = reader[CommonStrings.Name];
                        newRow[CommonStrings.IsStandard] = reader[CommonStrings.IsStandard];
                        newRow[CommonStrings.DocNamespace] = reader[CommonStrings.DocNamespace];
                        collectionDT.Rows.Add(newRow);
                    }
                }
                catch (SqlException e)
                {
                    SetMessage(Strings.ErrorLoadingArchivedDoc, e, "GetArchivedDocuments");
                }
                finally
                {
                    if (reader != null)
                        reader.Close();

                    if (command != null)
                        command.Dispose();

                    if (connection != null)
                    {
                        if (connection.State == ConnectionState.Open)
                            connection.Close();
                        connection.Dispose();
                    }
                }

                return collectionDT;
            }
        }

        //--------------------------------------------------------------------------------------------
        public CollectorsResultDataTable GetAllOCRTemplate(int collectorID)
        {
            CollectorsResultDataTable searchResult = new CollectorsResultDataTable();
            try
            {
                var v = from col in dc.DMS_Collectors
                        where col.Name != CommonStrings.EACollector && col.IsStandard == true
                        select col;

                foreach (DMS_Collector coll in v)
                {
                    DataRow newRow = searchResult.NewRow();
                    newRow[CommonStrings.CollectorID] = coll.CollectorID;
                    newRow[CommonStrings.Collector] = coll.Name;
                    newRow[CommonStrings.Collections] = coll.DMS_Collections.ToArray();
                    searchResult.Rows.Add(newRow);
                }
                return searchResult;
            }
            catch (Exception e)
            {
                SetMessage(Strings.ErrorLoadingArchivedDoc, e, "GetAllCollectors");
            }

            return null;
        }

        //--------------------------------------------------------------------------------
        public List<string> TokenizeSearchText(string searchText)
        {
            List<string> values = null;

            if (string.IsNullOrWhiteSpace(searchText))
                return values;

            //the user wants to search the entire phrase: i.e. "this is an entire phrase"
            if (searchText.Count() > 2 && searchText.First() == '"' && searchText.Last() == '"')
            {
                values = new List<string>();
                string value = searchText;
                value = value.Substring(1); // tolgo il primo "
                value = value.Remove(value.Count() - 1); // tolgo l'ultimo "
                values.Add(value);
            }
            else
            {
                //else first I tokenize the text and concatenate the single token in AND
                char[] delimier = { ' ', ',', ':', ';', '!', '?', '\'', '\"' };
                string[] tokens = searchText.Split(delimier, StringSplitOptions.RemoveEmptyEntries);
                values = tokens.ToList();
            }

            return values;
        }

        /// <summary>
        /// Build the where clause about the search free text in attachments
        /// </summary>
        //--------------------------------------------------------------------------------
        private void AddSearchTextClauseForAttachment(ref IQueryable<DMS_AttachmentSearchIndex> attachments, string token, SearchLocation location)
        {
            if (string.IsNullOrWhiteSpace(token) || location == SearchLocation.None)
                return;

            List<string> values = new List<string>();
            values.Add(token);

            if (location == SearchLocation.All)
            {
                attachments = attachments.Where(p => p.DMS_SearchFieldIndex.FieldValue.Contains(token) ||
                                                p.DMS_Attachment.DMS_ArchivedDocument.Barcode.Contains(token));


                return;
            }

            var checkValues = PredicateBuilder.False<DMS_AttachmentSearchIndex>();

            //if text is to search in Tags :
            //OR (AS.SearchFieldIndex.FieldName = "Tags")
            if ((location & SearchLocation.Tags) == SearchLocation.Tags)
                checkValues = checkValues.Or(p => p.DMS_SearchFieldIndex.FieldName == CommonStrings.Tags &&
                                                  p.DMS_SearchFieldIndex.FieldValue.Contains(token));

            //if text is to search in filename or description :
            //OR (AS.SearchFieldIndex.FieldName = "DescriptionTag" OR AS.SearchFieldIndex.FieldName = "FileNameTag")
            if ((location & SearchLocation.NameAndDescription) == SearchLocation.NameAndDescription)
                checkValues = checkValues.Or(p => (p.DMS_SearchFieldIndex.FieldName == CommonStrings.DescriptionTag ||
                                                p.DMS_SearchFieldIndex.FieldName == CommonStrings.FileNameTag) &&
                                                p.DMS_SearchFieldIndex.FieldValue.Contains(token));

            //if text is to search in Barcode :
            //OR (AS.SearchFieldIndex.FieldName = "BarcodeTag")
            if ((location & SearchLocation.Barcode) == SearchLocation.Barcode)
                checkValues = checkValues.Or(p => p.DMS_Attachment.DMS_ArchivedDocument.Barcode.Contains(token));

            //if text is to search in All bookmarks :			
            if ((location & SearchLocation.AllBookmarks) == SearchLocation.AllBookmarks)
                checkValues = checkValues.Or(p => p.DMS_SearchFieldIndex.FieldName != CommonStrings.Tags &&
                        p.DMS_SearchFieldIndex.FieldName != CommonStrings.FileNameTag &&
                        p.DMS_SearchFieldIndex.FieldName != CommonStrings.DescriptionTag &&
                        p.DMS_SearchFieldIndex.FieldName != CommonStrings.BarcodeTag &&
                        p.DMS_SearchFieldIndex.FieldValue.Contains(token));

            attachments = attachments.Where(checkValues);

        }

        /// <summary>
        /// Build the where clause about the search free text in archived documents
        /// </summary>
        //--------------------------------------------------------------------------------
        private void AddSearchTextClauseForArchiveDoc(ref IQueryable<DMS_ArchivedDocSearchIndex> archiveDocs, string token, SearchLocation location)
        {
            if (string.IsNullOrWhiteSpace(token) || location == SearchLocation.None)
                return;

            List<string> values = new List<string>();
            values.Add(token);

            if (location == SearchLocation.All)
            {
                archiveDocs = archiveDocs.Where(p => p.DMS_SearchFieldIndex.FieldValue.Contains(token));
                return;
            }

            var checkValues = PredicateBuilder.False<DMS_ArchivedDocSearchIndex>();

            //if text is to search in Tags :
            //OR (AS.SearchFieldIndex.FieldName = "Tags")
            if ((location & SearchLocation.Tags) == SearchLocation.Tags)
                checkValues = checkValues.Or(p => p.DMS_SearchFieldIndex.FieldName == CommonStrings.Tags);

            //if text is to search in Tags :
            //OR (AS.SearchFieldIndex.FieldName = "DescriptionTag" OR AS.SearchFieldIndex.FieldName = "FileNameTag")
            if ((location & SearchLocation.NameAndDescription) == SearchLocation.NameAndDescription)
                checkValues = checkValues.Or(p => p.DMS_SearchFieldIndex.FieldName == CommonStrings.DescriptionTag ||
                                                p.DMS_SearchFieldIndex.FieldName == CommonStrings.FileNameTag);

            //if text is to search in Barcode :
            //OR (AS.SearchFieldIndex.FieldName = "BarcodeTag")
            if ((location & SearchLocation.Barcode) == SearchLocation.Barcode)
                checkValues = checkValues.Or(p => p.DMS_SearchFieldIndex.FieldName == CommonStrings.BarcodeTag);

            // N.B. per la ricerca nei documenti archiviati non ricerco nei bookmark, quindi non considero SearchLocation.AllBookmarks
            //AND AS.SearchFieldIndex.FieldName like "token"		
            archiveDocs = archiveDocs.Where(checkValues);
            archiveDocs = archiveDocs.Where(p => p.DMS_SearchFieldIndex.FieldValue.Contains(token));
        }
        //used to write in DMS_IndexesSyncronization the changed bookmark fields of the current erp document
        //I run it on a new thread  to avoid a decrease of performance during saving erp document operation
        //-----------------------------------------------------------------------------------------------
        public void SyncronizationIndexes(int erpDocumentID, List<BookmarkToObserve> bookmarksToObserve)
        {
            if (erpDocumentID == -1 || bookmarksToObserve.Count == 0)
                return;
           
            FieldData fieldData = null;           
            DMSModelDataContext treadDc = null;
            try
            {
                treadDc = new DMSModelDataContext(DMSOrchestrator.DMSConnectionString);
                foreach (BookmarkToObserve bookmark in bookmarksToObserve)
                {
                    if (!bookmark.Changed)
                        continue;
                   
                    fieldData = new FieldData(bookmark.DataObj);
                    // cerco nella tabella DMS_SearchFieldIndexes quel fieldname + fieldvalue
                    var searchField = from sField
                                    in treadDc.DMS_SearchFieldIndexes
                                      where sField.FieldName == bookmark.FieldName &&
                                      sField.FieldValue == fieldData.StringValue
                                    select sField;

                    DMS_SearchFieldIndex dfi = null;

                    // se non ho trovato il record lo inserisco
                    if (searchField == null || !searchField.Any())
                    {
                        dfi = new DMS_SearchFieldIndex();
                        dfi.FieldName = bookmark.FieldName;
                        dfi.FieldValue = fieldData.StringValue;
                        dfi.FormattedValue = fieldData.FormattedValue;
                        treadDc.DMS_SearchFieldIndexes.InsertOnSubmit(dfi);
                        treadDc.SubmitChanges();
                    }
                    else
                        // mi tengo da parte il SearchIndexID letto dal db
                        dfi = (DMS_SearchFieldIndex)searchField.Single();

                    // identifico i record della DMS_AttachmentSearchIndexes che non sono piu' validi
                    var attSearchIdx = from searchIdx
                                       in treadDc.DMS_AttachmentSearchIndexes
                                       where searchIdx.DMS_Attachment.ErpDocumentID == erpDocumentID &&
                                       searchIdx.DMS_SearchFieldIndex.FieldName == bookmark.FieldName
                                       select searchIdx;

                    // se ne ho trovati li aggiorno con il nuovo searchID appena inserito
                    if (attSearchIdx != null && attSearchIdx.Any())
                    {
                        int attID;
                        DMS_AttachmentSearchIndex asi;

                        foreach (DMS_AttachmentSearchIndex attach in attSearchIdx)
                        {
                            // mi tengo da parte l'AttachmentID
                            attID = attach.AttachmentID;
                            // cancello il record corrente
                            treadDc.DMS_AttachmentSearchIndexes.DeleteOnSubmit(attach);

                            // inserisco il nuovo record con il SearchID aggiornato
                            asi = new DMS_AttachmentSearchIndex();
                            asi.AttachmentID = attID;
                            asi.SearchIndexID = dfi.SearchIndexID;
                            treadDc.DMS_AttachmentSearchIndexes.InsertOnSubmit(asi);
                            treadDc.SubmitChanges();
                        }
                    }
                }

            }
            catch (SqlException sqlExc)
            {
                SetMessage(Strings.WriteIndexesSynchronizationError, sqlExc, "SyncronizationIndexesThread");
            }
            catch (Exception e)
            {
                SetMessage(Strings.WriteIndexesSynchronizationError, e, "SyncronizationIndexesThread");
            }
            finally
            {
                if (treadDc != null)
                    treadDc.Dispose();

                if (SyncronizationIndexesFinished != null)
                    SyncronizationIndexesFinished(this, EventArgs.Empty);
            }
        }
        
        //--------------------------------------------------------------------------------
        private IEnumerable<DMS_Attachment> GetSearchTextResult(string searchText, SearchLocation location)
        {
            IEnumerable<DMS_Attachment> attResult = null;
            IEnumerable<DMS_Attachment> singleTokenResult = null;
            IEnumerable<DMS_Attachment> contentResult = null;

            //for each token in searchText I must create a query and then intersect its result with the previous result
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                if (location != SearchLocation.Content)
                {
                    List<string> values = TokenizeSearchText(searchText);

                    foreach (string token in values)
                    {
                        string temp = token;
                        IQueryable<DMS_AttachmentSearchIndex> attachments = CreateBaseSearchQuery();
                        AddSearchTextClauseForAttachment(ref attachments, temp, location);

                        singleTokenResult = (attachments.Select(a => a.DMS_Attachment).AsEnumerable()).Distinct(new DistinctAttachmentIDAttachment());

                        if (singleTokenResult == null || !singleTokenResult.Any())
                        {
                            attResult = null;
                            break; //non ho trovato niente
                        }

                        attResult = (attResult == null)
                                ? singleTokenResult
                                : attResult.Intersect(singleTokenResult, new DistinctAttachmentIDAttachment());
                    }
                }
                //se l'utente ha chiesto anche la ricerca FullText allora devo fare una query sulla colonna ContentText
                //su cui è stato costruito un indice di ricerca via SqlServer
                if ((location == SearchLocation.All || (location & SearchLocation.Content) == SearchLocation.Content))
                {
                    try
                    {
                        contentResult = FullTextFilterManager.SearchForAttachments(SearchRules, searchText, DMSOrchestrator);
                    }
                    catch (SqlException e)
                    {
                        //if (e.Number == 30046)
                        throw (e);
                    }
                    catch (Exception e)
                    {
                        throw (e);
                    }

                    // se l'utente ha ricercato non solo nel contenuto del documento
                    if (attResult != null)
                        attResult = (contentResult != null) ? attResult.Union(contentResult) : attResult;
                    else
                        attResult = contentResult;

                    if (attResult != null)
                        attResult = attResult.Distinct(new DistinctAttachmentIDAttachment());
                }
            }

            return attResult;
        }

        //--------------------------------------------------------------------------------
        private IEnumerable<DMS_Attachment> GetBookmarksSearchResult(SearchFieldsDataTable searchPatternDT)
        {
            if (searchPatternDT == null || searchPatternDT.Rows.Count == 0)
                return null;

            var orderedResult = searchPatternDT.AsEnumerable().OrderBy(f => f[CommonStrings.Name]);

            List<String> fieldValue = new List<String>();
            string fieldName = string.Empty;
            string oldName = string.Empty;

            IEnumerable<DMS_Attachment> attResult = null;
            IEnumerable<DMS_Attachment> singleBookmarkResult = null;

            try
            {
                foreach (DataRow row in orderedResult)
                {
                    // first step: I create the first search query considering collection, erp document information, attachment date 
                    // and extension type 
                    fieldName = row[CommonStrings.Name].ToString();
                    if (string.Compare(fieldName, oldName, StringComparison.InvariantCultureIgnoreCase) != 0)
                    {
                        if (fieldValue.Count() > 0)
                        {
                            IQueryable<DMS_AttachmentSearchIndex> attachments = CreateBaseSearchQuery();
                            attachments = attachments.Where(DMS_AttachmentSearchIndex.EqualsTo(oldName, fieldValue));
                            singleBookmarkResult = (attachments.Select(a => a.DMS_Attachment).AsEnumerable()).Distinct(new DistinctAttachmentIDAttachment());
                            attResult = (attResult == null)
                                ? singleBookmarkResult
                                : attResult.Intersect(singleBookmarkResult, new DistinctAttachmentIDAttachment());
                        }
                        oldName = fieldName;
                        fieldValue.Clear();
                    }
                    fieldValue.Add(((FieldData)row[CommonStrings.FieldData]).StringValue);
                }
                //per l'ultimo bookmark (o l'unico presente in searchPatternDT)
                if (fieldValue.Count() > 0)

                {
                    IQueryable<DMS_AttachmentSearchIndex> attachments = CreateBaseSearchQuery();
                    attachments = attachments.Where(DMS_AttachmentSearchIndex.EqualsTo(fieldName, fieldValue));
                    singleBookmarkResult = (attachments.Select(a => a.DMS_Attachment).AsEnumerable()).Distinct(new DistinctAttachmentIDAttachment());
                    attResult = (attResult == null)
                            ? singleBookmarkResult
                            : attResult.Intersect(singleBookmarkResult, new DistinctAttachmentIDAttachment());
                }
            }
            catch (Exception e)
            {
                throw (e);
            }
            return attResult;
        }

        //--------------------------------------------------------------------------------        
        public SearchFieldsDataTable CreateSearchFieldsDataTable(string searchFields)
        {
            SearchFieldsDataTable searchFieldsDT = new SearchFieldsDataTable();
            if (string.IsNullOrWhiteSpace(searchFields) || string.IsNullOrEmpty(searchFields))
                return searchFieldsDT;

            SqlConnection connection = null;
            SqlCommand command = null;
            SqlParameter fieldNameParam = null;
            try
            {
                connection = new SqlConnection(DMSOrchestrator.DMSConnectionString);
                connection.Open();
                command = new SqlCommand();
                command.Connection = connection;
                command.CommandText = "Select ValueType from DMS_Field where FieldName = @fieldName";
                fieldNameParam = new SqlParameter("@fieldName", SqlDbType.VarChar, 50);
                command.Parameters.Add(fieldNameParam);
                command.Prepare();

                char[] delimier = { ';' };
                string[] tokens = searchFields.Split(delimier, StringSplitOptions.RemoveEmptyEntries);
                List<string> values = tokens.ToList();
                int pos = -1;
                string field = string.Empty;
                string value = string.Empty;
                string type = string.Empty;
                foreach (string token in values)
                {
                    pos = token.IndexOf(':');
                    if (pos > 0)
                    {
                        field = token.Substring(0, pos);
                        value = token.Substring(pos + 1);
                        fieldNameParam.Value = field;
                        type = (string)command.ExecuteScalar();
                        searchFieldsDT.AddSearchFieldValue(field, value, type, true);
                    }
                }
            }
            catch (SqlException e)
            {
                SetMessage(Strings.ErrorLoadingArchivedDoc, e, "GetArchivedDocuments");
            }
            catch (Exception ex)
            {
                SetMessage(Strings.ErrorLoadingArchivedDoc, ex, "GetArchivedDocuments");
            }
            finally
            {
                if (command != null)
                    command.Dispose();

                if (connection != null)
                {
                    if (connection.State == ConnectionState.Open)
                        connection.Close();
                    connection.Dispose();
                }
            }

            return searchFieldsDT;
        }


        //chiamati dal mondo unmanaged
        //--------------------------------------------------------------------------------
        public List<AttachmentInfo> SearchAttachments(string collectorName, string collectionName, string extType, DateTime startDate, DateTime endDate, string searchText, SearchLocation location, SearchFieldsDataTable selectedSearchFields)
        {
            if (
                    (string.IsNullOrEmpty(collectorName) && string.IsNullOrEmpty(collectionName)) ||
                    (string.Compare(collectorName, "All", true) == 0 && string.Compare(collectionName, "All", true) == 0)
                )
            {
                SearchRules.SearchContext = SearchContext.Repository;
                SearchRules.CollectorName = string.Empty;
                SearchRules.CollectionName = string.Empty;
            }
            else
            {
                if (
                        (!string.IsNullOrEmpty(collectorName) && string.Compare(collectorName, "All", true) != 0) &&
                        (string.IsNullOrEmpty(collectionName) || string.Compare(collectionName, "All", true) == 0)
                    )
                {
                    SearchRules.SearchContext = SearchContext.Collector;
                    SearchRules.CollectorName = collectorName;
                    SearchRules.CollectionName = string.Empty;
                }
                else
                {
                    SearchRules.SearchContext = SearchContext.Collection;
                    SearchRules.CollectorName = collectorName;
                    SearchRules.CollectionName = collectionName;
                }

            }

            SearchRules.DocumentNamespace = string.Empty;
            SearchRules.DocumentPrimaryKey = string.Empty;
            SearchRules.DocExtensionType = extType;
            SearchRules.StartDate = startDate;
            SearchRules.EndDate = endDate;
            SearchResultDataTable searchFieldDT = SearchThread(searchText, location, selectedSearchFields);
            List<AttachmentInfo> attInfoList = new List<AttachmentInfo>();
            foreach (DataRow row in searchFieldDT.Rows)
                attInfoList.Add((AttachmentInfo)row[CommonStrings.AttachmentInfo]);

            return attInfoList;
        }
        //--------------------------------------------------------------------------------
        public List<AttachmentInfo> SearchAttachmentsForDocument(string docNamespace, string docPrimaryKey, string searchText, SearchLocation location, SearchFieldsDataTable selectedSearchFields)
        {
            List<AttachmentInfo> attInfoList = new List<AttachmentInfo>();
            if (!CheckNamespaceAndPrimaryKey(ref docNamespace, docPrimaryKey,string.Empty))
                return attInfoList;

            SearchRules.DocumentNamespace = docNamespace;
            SearchRules.DocumentPrimaryKey = docPrimaryKey;
            SearchResultDataTable searchFieldDT = SearchThread(searchText, location, selectedSearchFields);
            foreach (DataRow row in searchFieldDT.Rows)
                attInfoList.Add((AttachmentInfo)row[CommonStrings.AttachmentInfo]);

            return attInfoList;
        }

        //--------------------------------------------------------------------------------
        public void Search(string searchText, SearchLocation location, SearchFieldsDataTable selectedSearchFields, string docNamespace, string docPrimaryKey)
        {
            while (startSearchThread)
                Thread.Sleep(1500);
            startSearchThread = true;

            //security integration
            if (DMSOrchestrator.SecurityEnabled)
                LoadERPDocumentsGrant();

            SearchRules.DocumentNamespace = docNamespace;
            SearchRules.DocumentPrimaryKey = docPrimaryKey;

            Thread thread = new Thread(() =>
            {
                SearchThread(searchText, location, selectedSearchFields); startSearchThread = false;
            });

            thread.SetApartmentState(ApartmentState.STA);
            // quando si istanzia un nuovo Thread bisogna assegnargli le CurrentCulture, altrimenti le
            // traduzioni in lingue differenti da quelle del sistema operativo non funzionano!!!
            thread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
            thread.CurrentCulture = Thread.CurrentThread.CurrentCulture;

            thread.Start();
        }

        //--------------------------------------------------------------------------------
        public string GetDocumentKeyDescription(int attachmentID)
        {
            string description = string.Empty;
            try
            {
                var descriField = (from a in dc.DMS_AttachmentSearchIndexes
                                   join s in dc.DMS_SearchFieldIndexes on a.SearchIndexID equals s.SearchIndexID
                                   join c in dc.DMS_CollectionsFields on a.DMS_Attachment.CollectionID equals c.CollectionID
                                   where a.AttachmentID == attachmentID && c.ShowAsDescription == true && s.FieldName == c.FieldName
                                   select new
                                   {
                                       Descri = c.DMS_Field.FieldDescription,
                                       Value = s.FormattedValue //valore formattato
                                   });

                if (descriField != null && descriField.Any())
                    foreach (var f in descriField)
                        description += string.Format(" {0}:{1}", f.Descri, f.Value);



            }
            catch (Exception e)
            {
                SetMessage(Strings.ErrorSearchingFieldValues, e, "GetDocumentKeyDescription");
            }

            return description;
        }

        // se l'utente effettua una ricerca semplice senza il testo libero e bookmark allora faccio una query su DMS_Attachments 
        // senza considerare gli indici di ricerca
        //--------------------------------------------------------------------------------
        internal IEnumerable<DMS_Attachment> GetSimpleSearchResult(bool onlySearchContext)
        {
            IQueryable<DMS_Attachment> attachments = dc.DMS_Attachments;
            IEnumerable<DMS_Attachment> simpleResult = null;
            try
            {
                switch (SearchRules.SearchContext)
                {
                    case SearchContext.Current:
                        attachments = attachments.Where(att => att.DMS_ErpDocument.DocNamespace == SearchRules.DocumentNamespace &&
                                     att.DMS_ErpDocument.PrimaryKeyValue == SearchRules.DocumentPrimaryKey);
                        break;

                    case SearchContext.Collection:
                        attachments = attachments.Where(att => att.DMS_Collection.DMS_Collector.Name == SearchRules.CollectorName &&
                                     att.DMS_Collection.Name == SearchRules.CollectionName);
                        break;

                    case SearchContext.Collector:
                        attachments = attachments.Where(att => att.DMS_Collection.DMS_Collector.Name == SearchRules.CollectorName);
                        break;

                    case SearchContext.Custom:
                        {
                            if (SearchRules.CustomFilters != null)
                            {
                                var checkValues = PredicateBuilder.False<DMS_Attachment>();
                                foreach (KeyValuePair<int, IList<int>> i in SearchRules.CustomFilters)
                                    foreach (int ii in i.Value as List<int>)
                                    {
                                        int temp = ii;
                                        checkValues = checkValues.Or(p => (p.CollectionID == temp));
                                    }
                                attachments = attachments.Where(checkValues);
                            }
                        }
                        break;

                }
                if (onlySearchContext)
                    return (attachments != null) ? (attachments.Select(a => a).AsEnumerable()) : null;

                //date filter
                if (SearchRules.StartDate != DateTime.MinValue && SearchRules.EndDate != DateTime.MaxValue)
                    attachments = attachments.Where(a => (a.TBModified >= SearchRules.StartDate && a.TBModified <= SearchRules.EndDate) ||
                                                         (a.TBCreated >= SearchRules.StartDate && a.TBCreated <= SearchRules.EndDate));

                if (
                        !string.IsNullOrEmpty(SearchRules.DocExtensionType) &&
                        string.Compare(SearchRules.DocExtensionType, CommonStrings.AllExtensions, StringComparison.InvariantCultureIgnoreCase) != 0
                    )
                    attachments = attachments.Where(a => a.DMS_ArchivedDocument.ExtensionType == SearchRules.DocExtensionType);

                simpleResult = (attachments.Select(a => a).AsEnumerable());

            }
            catch (Exception e)
            {
                SetMessage(Strings.ErrorCreatingBaseSearchResult, e, "GetSimpleSearchQuery");
            }

            return simpleResult;
        }

        //--------------------------------------------------------------------------------
        private void CompleteSearch(ref IEnumerable<DMS_Attachment> attResults, ref SearchResultDataTable searchResultDT)
        {
            try
            {
                if (attResults != null)
                {
                    DataRow newRow = null;
                    foreach (DMS_Attachment attachment in attResults)
                    {
                        if (attachment == null || attachment.DMS_ArchivedDocument == null || attachment.DMS_ErpDocument == null)
                            continue;

                        newRow = searchResultDT.NewRow();
                        newRow[CommonStrings.ArchivedDocID] = attachment.ArchivedDocID;
                        newRow[CommonStrings.AttachmentID] = attachment.AttachmentID;
                        newRow[CommonStrings.Collector] = attachment.DMS_Collection.DMS_Collector.CollectorID;
                        newRow[CommonStrings.Collection] = attachment.DMS_Collection.CollectionID;
                        newRow[CommonStrings.DocNamespace] = attachment.DMS_ErpDocument.DocNamespace;
                        newRow[CommonStrings.TBPrimaryKey] = attachment.DMS_ErpDocument.PrimaryKeyValue;
                        newRow[CommonStrings.Name] = attachment.DMS_ArchivedDocument.Name;
                        newRow[CommonStrings.AttachmentDate] = (attachment.TBCreated == null) ? DateTime.MinValue : attachment.TBCreated;
                        newRow[CommonStrings.ExtensionType] = attachment.DMS_ArchivedDocument.ExtensionType;
                        newRow[CommonStrings.DocKeyDescription] = GetDocumentKeyDescription(attachment.AttachmentID);
                        newRow[CommonStrings.AttachmentInfo] = new AttachmentInfo(attachment, DMSOrchestrator);
                        searchResultDT.Rows.Add(newRow);

                        if (OnAddRowToResult != null)
                            OnAddRowToResult(this, new AddRowInResultEventArgs(newRow));
                    }
                }

                //filter 
                if (searchResultDT == null || searchResultDT.Rows.Count <= 0)
                    return;

                GroupResult(searchResultDT);
            }
            catch (Exception e)
            {
                throw (e);
            }
        }

        /*The search result is obtained by the INTERSECTION of two different queries
	  First query used to extract the records metting the condition of the free text
	  Second query used to extract the records metting the condition of the specified bookmarks values
	  For each query we also consider the search context, data and document type choosed by the user. This part of
	  query is composed by CreateBaseQuery method.		
	  The complete query is something like this:
	  SELECT DISTINCT(AS.AttachmentID) FROM DMS_AttachmentSearchIndex AS ASI, DMS_SearchFieldIndex AS SF  DMS_Attachment as A, DMS_Collector as C, DMS_Collection as C1, DMS_ErpDocument as E, DMS_ArchivedDocument as AD 
		WHERE ASI.SearchFieldID = SF.SearchFieldID AND A.AttachmentID = ASI.AttachmentID AND A.CollectionID = C1.CollectionID AND C1.CollectorID = C.CollectorID AND A.ArchivedDocID =  D.ArchivedDocID AND A.ErpDocumentID = E.ErpDocumentID 
		AND C.Name = 'SearchRules.CollectorName' AND C1.Name = "SearchRules.CollectionName" AND E.DocNamespace = "DMSOrchestrator.DocumentNamespace" AND E.PrimaryKey = "DMSOrchestrator.DocumentPrimaryKey"
		AND A.TBCreated >= SearchRules.StartDate AND A.TBCreated <= SearchRules.EndDate  
		AND D.ExtensionType == SearchRules.DocExtensionType 
		AND (ASI.SearchFieldIndex.FieldValue = 'freeTextToken1' AND ASI.SearchFieldIndex.FieldValue = 'freeTextToken2' AND ....) 	
	  INTERSECT
	  SELECT DISTINCT(AS.AttachmentID) FROM DMS_AttachmentSearchIndex AS ASI, DMS_SearchFieldIndex AS SF  DMS_Attachment as A, DMS_Collector as C, DMS_Collection as C1, DMS_ErpDocument as E, DMS_ArchivedDocument as AD 
		WHERE ASI.SearchFieldID = SF.SearchFieldID AND A.AttachmentID = ASI.AttachmentID AND A.CollectionID = C1.CollectionID AND C1.CollectorID = C.CollectorID AND A.ArchivedDocID =  D.ArchivedDocID AND A.ErpDocumentID = E.ErpDocumentID 
		AND C.Name = 'SearchRules.CollectorName' AND C1.Name = "SearchRules.CollectionName" AND E.DocNamespace = "DMSOrchestrator.DocumentNamespace" AND E.PrimaryKey = "DMSOrchestrator.DocumentPrimaryKey"
		AND A.TBCreated >= SearchRules.StartDate AND A.TBCreated <= SearchRules.EndDate  
		AND D.ExtensionType == SearchRules.DocExtensionType
		AND (ASI.SearchFieldIndex.FieldName = 'Name1' AND (ASI.SearchFieldIndex.FieldValue = 'Value21' OR ASI.SearchFieldIndex.FieldValue = 'Value22' OR ....) 	
		AND (ASI.SearchFieldIndex.FieldName = 'Name2' AND (ASI.SearchFieldIndex.FieldValue = 'Value21' OR ASI.SearchFieldIndex.FieldValue = 'Value22' OR ....) 		
		 * */
        //--------------------------------------------------------------------------------
        private SearchResultDataTable SearchThread(string freeText, SearchLocation location, SearchFieldsDataTable searchPatternDT)
        {
            SearchResultDataTable searchResultDT = new SearchResultDataTable();
            try
            {
                if (SearchResultCleared != null)
                    SearchResultCleared(this, EventArgs.Empty);


                //se l'utente ha tolto il check da tutte le searchlocation allora come testo da ricercare considero string.Empty, qeusto vuol dire che
                //l'algoritmo di ricerca non ricercherà per testo ma solo considerando gli altri criteri (data, tipo, profondità ed eventuali bookmark)
                string searchText = (location == SearchLocation.None) ? string.Empty : freeText;

                IEnumerable<DMS_Attachment> attResults = null;
                if (string.IsNullOrWhiteSpace(searchText) && (searchPatternDT == null || searchPatternDT.Rows.Count == 0))
                    attResults = GetSimpleSearchResult(false);
                else
                {
                    IEnumerable<DMS_Attachment> searchTextResult = null;
                    IEnumerable<DMS_Attachment> bookmarksResult = null;

                    //First I create the result for searchText criteria
                    if (!string.IsNullOrWhiteSpace(searchText))
                        searchTextResult = GetSearchTextResult(searchText, location);

                    //then the result for bookmarks criteria
                    if (searchPatternDT != null && searchPatternDT.Rows.Count > 0)
                    {
                        bookmarksResult = GetBookmarksSearchResult(searchPatternDT);

                        if (!string.IsNullOrWhiteSpace(searchText))
                        {
                            attResults = (searchTextResult != null && searchTextResult.Any())
                                        ? searchTextResult.Intersect(bookmarksResult, new DistinctAttachmentIDAttachment())
                                        : null;
                        }
                        else
                            attResults = bookmarksResult;
                    }
                    else
                        attResults = searchTextResult;
                }

                CompleteSearch(ref attResults, ref searchResultDT);



            }
            catch (SqlException e)
            {
                //SQL Server encountered error 0x80070218 while communicating with full-text filter daemon host (FDHost) process. 
                //Make sure that the FDHost process is running. To re-start the FDHost process, run the sp_fulltext_service 
                //'restart_all_fdhosts' command or restart the SQL Server instance.
                if (e.Number == 30046)
                    SetMessage(Strings.ErrorFDHostProcess, null, "SearchAttachmentThread");
                else
                    SetMessage(Strings.ErrorSearchingAttachment, e, "SearchAttachmentThread");
            }
            catch (Exception e)
            {
                SetMessage(Strings.ErrorSearchingAttachment, e, "SearchAttachmentThread");
            }
            finally
            {
                if (SearchFinished != null)
                    SearchFinished(this, EventArgs.Empty);
            }

            return searchResultDT;
        }

        /// <summary>
        /// dato un archivedDocId ricerca tutti i documenti di ERP di cui è un attachment
        /// </summary>
        /// <param name="archivedDocId"></param>
        //--------------------------------------------------------------------------------
        public SearchResultDataTable GetERPDocumentAttachment(int archivedDocId)
        {
            SearchResultDataTable resultDT = new SearchResultDataTable();

            try
            {
                //dc.Refresh(RefreshMode.OverwriteCurrentValues, dc.DMS_Attachments);

                var attachments = from a in dc.DMS_Attachments
                                  where a.ArchivedDocID == archivedDocId
                                  orderby a.ErpDocumentID
                                  select a;

                foreach (DMS_Attachment att in attachments)
                {
                    //security integration
                    if (DMSOrchestrator.SecurityEnabled && !CheckSecurityOnDocNamespace(att.DMS_ErpDocument.DocNamespace))
                        continue;

                    DataRow row = resultDT.NewRow();
                    row[CommonStrings.ArchivedDocID] = att.ArchivedDocID;
                    row[CommonStrings.AttachmentID] = att.AttachmentID;
                    row[CommonStrings.TBPrimaryKey] = att.DMS_ErpDocument.PrimaryKeyValue;
                    row[CommonStrings.DocNamespace] = att.DMS_ErpDocument.DocNamespace;
                    row[CommonStrings.DocKeyDescription] = GetDocumentKeyDescription(att.AttachmentID);
                    resultDT.Rows.Add(row);
                }
            }
            catch (Exception e)
            {
                SetMessage(Strings.ErrorSearchingAttachment, e, "GetERPDocumentAttachment");
            }

            return resultDT;
        }


        //--------------------------------------------------------------------------------
        internal List<string> GetAllExtensions()
        {
            List<string> searchResult = new List<string>();

            try
            {
                var v = (from col in dc.DMS_ArchivedDocuments select col.ExtensionType).Distinct();

                foreach (string ext in v)
                    searchResult.Add(ext);
            }
            catch (Exception e)
            {
                SetMessage(Strings.ErrorLoadingArchivedDoc, e, "GetAllExtensions");
            }

            return searchResult;
        }

        //---------------------------------------------------------------------
        internal bool IsAttached(int archivedDocId)
        {
            var attachments = (from att in dc.DMS_Attachments
                               where att.ArchivedDocID == archivedDocId
                               select att.AttachmentID);

            return (attachments != null && attachments.Any());
        }

        /// <summary>
        /// return the DMS_Attachment with primary key = attachmentId
        /// </summary>
        /// <param name="attachmentID"></param>
        /// <returns></returns>
        //---------------------------------------------------------------------
        public DMS_Attachment GetAttachment(int attachmentId)
        {
            var attachment = (from att in dc.DMS_Attachments
                              where att.AttachmentID == attachmentId
                              select att);

            return (attachment != null && attachment.Any()) ? (DMS_Attachment)attachment.Single() : null;
        }

        //---------------------------------------------------------------------
        public DMS_ArchivedDocument GetArchivedDocument(int archivedDocId)
        {
            var archived = (from att in dc.DMS_ArchivedDocuments
                            where att.ArchivedDocID == archivedDocId
                            select att);

            return (archived != null && archived.Any()) ? (DMS_ArchivedDocument)archived.Single() : null;
        }

        //---------------------------------------------------------------------
        public DMS_Collection GetCollection(int collectionID)
        {
            var collection = (from coll in dc.DMS_Collections
                              where coll.CollectionID == collectionID
                              select coll);
            return (collection != null && collection.Any()) ? (DMS_Collection)collection.Single() : null;
        }

        //-----------------------------------------------------------------------------------------------
        public DMS_Collection GetCollection(string collectorName, string collectionName, string templateName = CommonStrings.DefaultTemplate)
        {
            var collection = from coll in dc.DMS_Collections
                             where
                             coll.DMS_Collector.Name == collectorName &&
                             coll.Name == collectionName &&
                             coll.TemplateName == templateName
                             select coll;
            return (collection != null && collection.Any()) ? (DMS_Collection)collection.Single() : null;
        }

        //---------------------------------------------------------------------
        public DMS_Attachment GetAttachmentForArchivedDocId(int archivedDocId, string docNamespace, string primaryKey)
        {
            var attachment = (from att in dc.DMS_Attachments
                              where att.ArchivedDocID == archivedDocId &&
                              att.DMS_ErpDocument.DocNamespace == docNamespace &&
                              att.DMS_ErpDocument.PrimaryKeyValue == primaryKey
                              select att);

            return (attachment != null && attachment.Any()) ? (DMS_Attachment)attachment.Single() : null;
        }

        //--------------------------------------------------------------------------------
        public int GetAttachmentIDByFileName(string documentNamespace, string documentKey, string fileName)
        {
            try
            {
                var attach = from a in dc.DMS_Attachments
                             where a.DMS_ErpDocument.DocNamespace == documentNamespace && a.DMS_ErpDocument.PrimaryKeyValue == documentKey &&
                             a.DMS_ArchivedDocument.Name == fileName
                             orderby a.TBModified descending
                             select a.AttachmentID;

                if (attach != null && attach.Any())
                    return (int)attach.First();
            }
            catch (Exception e)
            {
                SetMessage(Strings.ErrorSearchingFieldValues, e, "GetAttachmentIDByFileName");
            }

            return -1;
        }

        //--------------------------------------------------------------------------------
        private string GetCommonBaseFilter()
        {
            string commonBaseFilter = string.Empty;

			if (fea.CollectionID > -1) //filtro per collection
				commonBaseFilter += string.Format("  A.CollectionID = '{0}' ", fea.CollectionID.ToString());

			//Options.RepositoryOptionsState.ShowOnlyMyArchivedDocs
			if (DMSOrchestrator.SettingsManager.UsersSettingState.Options.RepositoryOptionsState.ShowOnlyMyArchivedDocs && !DMSOrchestrator.IsAdmin)
			{
				if (!string.IsNullOrEmpty(commonBaseFilter))
					commonBaseFilter += " AND ";
				commonBaseFilter += string.Format(@"(D.TBCreatedID = {0} OR D.TBModifiedID = {0})", DMSOrchestrator.WorkerId.ToString());
			}
			else
			{
				//Worker filter
				if (fea.Workers != null && fea.Workers.Count > 0)
				{
					if (!string.IsNullOrEmpty(commonBaseFilter))
						commonBaseFilter += " AND ";
					commonBaseFilter += string.Format(@"D.TBCreatedID IN ( {0} )", string.Join<int>(",", fea.Workers));
				}
			}

            //extensions filter
            if (fea.DocExtensionType != CommonStrings.AllExtensions)
            {
                if (!string.IsNullOrEmpty(commonBaseFilter))
                    commonBaseFilter += " AND ";
                commonBaseFilter += string.Format(@"D.ExtensionType = '{0}'", fea.DocExtensionType);
            }			

			return commonBaseFilter;
        }

        //--------------------------------------------------------------------------------
        private string GetCommonFreeTagTokenFilter(string freeTagToken, string freeTagTokenParam, bool isAttachment)
        {
            string commonFreeTagTokenFilter = string.Empty;

            if (!string.IsNullOrEmpty(freeTagToken))
            {
                commonFreeTagTokenFilter += (isAttachment) ? " A.AttachmentID = I.AttachmentID" : " D.ArchivedDocID = I.ArchivedDocID";
                commonFreeTagTokenFilter += " AND S.SearchIndexID = I.SearchIndexID";

				commonFreeTagTokenFilter += " AND S.FieldValue LIKE " + freeTagTokenParam;
				if (!sqlCommand.Parameters.Contains(freeTagTokenParam))
					sqlCommand.Parameters.Add(new SqlParameter(freeTagTokenParam, '%' + freeTagToken + '%'));

				if (fea.SearchLocation != SearchLocation.All)
                {   //se eseguo ricerche specifiche allora  devo verificare singolarmente i campi
                    if ((fea.SearchLocation & SearchLocation.AllBookmarks) == SearchLocation.AllBookmarks)
                    {
                        string excludedFields = string.Empty;
                        //se cerco in allbookmark escludo i campi che non voglio
                        if ((fea.SearchLocation & SearchLocation.Tags) != SearchLocation.Tags)
                            excludedFields = "'" + CommonStrings.Tags + "'";
                        if ((fea.SearchLocation & SearchLocation.NameAndDescription) != SearchLocation.NameAndDescription)
                        {
                            if (!string.IsNullOrEmpty(excludedFields))
                                excludedFields += ", ";

                            excludedFields += "'" + CommonStrings.DescriptionTag + "'" + ", ";
                            excludedFields += "'" + CommonStrings.FileNameTag + "'";
                        }
						//il BarcodeTag è presente solo nella tabella DMS_ArchivedDocSearchIndexes perchè è legato al documento archiviato
						if ((fea.SearchLocation & SearchLocation.Barcode) != SearchLocation.Barcode)
							excludedFields = "'" + CommonStrings.BarcodeTag + "'";

						if (!string.IsNullOrEmpty(excludedFields))
							commonFreeTagTokenFilter += string.Format(@" AND S.FieldName NOT IN ({0})", excludedFields);
                    }

                    else//se non cerco in allbookmark allora includo i campi che voglio
                    {
                        string includedFields = string.Empty;

                        if ((fea.SearchLocation & SearchLocation.Tags) == SearchLocation.Tags)
                            includedFields = "'" + CommonStrings.Tags + "'";

                        if ((fea.SearchLocation & SearchLocation.NameAndDescription) == SearchLocation.NameAndDescription)
                        {
                            if (!string.IsNullOrEmpty(includedFields))
                                includedFields += ", ";

                            includedFields += "'" + CommonStrings.DescriptionTag + "'" + ", ";
                            includedFields += "'" + CommonStrings.FileNameTag + "'";
                        }

						if ((fea.SearchLocation & SearchLocation.Barcode) == SearchLocation.Barcode)
						{
							if (!string.IsNullOrEmpty(includedFields))
								includedFields += ", ";

							includedFields += "'" + CommonStrings.BarcodeTag + "'";
						}
						if (!string.IsNullOrEmpty(includedFields))
							commonFreeTagTokenFilter += string.Format(@" AND S.FieldName IN ({0})", includedFields);
                    }
                }
            }
            return commonFreeTagTokenFilter;
        }

        //--------------------------------------------------------------------------------
        private string GetBaseSingleConditionFilter(DataRow conditionRow)
        {
            string indexCond = string.Empty;
            List<Int32> values = (List<Int32>)conditionRow[CommonStrings.SearchIndexID];
            foreach (int ind in values)
            {
                if (!string.IsNullOrEmpty(indexCond))
                    indexCond += " OR ";
                indexCond += string.Format(@"SearchIndexID = {0}", ind.ToString());
            }
      
            return (values.Count > 1) ? " (" + indexCond + ") " : indexCond;
        }

        //--------------------------------------------------------------------------------
        private string GetArchivedDocBaseFilter()
        {
            string baseFilter = string.Empty;

            //date range
            if (fea.StartDate != DateTime.MinValue && fea.EndDate != DateTime.MaxValue)
            {
                baseFilter = string.Format(@"((D.TBModified BETWEEN @startDate AND @endDate ) OR (D.TBCreated BETWEEN @startDate AND @endDate))");
                if (!sqlCommand.Parameters.Contains("@startDate"))
                {
                    sqlCommand.Parameters.Add(new SqlParameter("@startDate", fea.StartDate));
                    sqlCommand.Parameters.Add(new SqlParameter("@endDate", fea.EndDate));
                }
            }
            string commonBaseFilter = GetCommonBaseFilter();
            if (!string.IsNullOrEmpty(commonBaseFilter))
            {
                if (!string.IsNullOrEmpty(baseFilter))
                    baseFilter += " AND ";
                baseFilter += commonBaseFilter;
            }
            return baseFilter;
        }


		//--------------------------------------------------------------------------------
		private string GetArchivedDocSingleCmdText(string freeTagToken, string freeTagTokenParam)
		{
			string archDocCommandText = string.Empty;

            archDocCommandText = archivedDocBaseSelected;

            if (!string.IsNullOrEmpty(freeTagToken))
                //aggiungo le tabelle relative agli indici di ricerca
                archDocCommandText += ", DMS_ArchivedDocSearchIndexes I, DMS_SearchFieldIndexes S";

            archDocCommandText += " WHERE ";
            string filter = GetArchivedDocBaseFilter();
            if (!string.IsNullOrEmpty(freeTagToken))
            {
                if (!string.IsNullOrEmpty(filter))
                    filter += " AND ";
                filter += GetCommonFreeTagTokenFilter(freeTagToken, freeTagTokenParam, false);
            }
            archDocCommandText += filter;

			archDocCommandText += archivedDocGroupBy;

			return archDocCommandText;
        }

        //--------------------------------------------------------------------------------
        private string GetArchivedDocAdvancedCmdText(DataRow conditionRow)
        {
            if (conditionRow == null)
                return string.Empty;

            string advancedCommandText = string.Empty;
            advancedCommandText = archivedDocBaseSelected;
            advancedCommandText += ", DMS_ArchivedDocSearchIndexes I";
            advancedCommandText += " WHERE ";

			string filter = GetArchivedDocBaseFilter();
            if (!string.IsNullOrEmpty(filter))
                filter += " AND ";
            filter += " D.ArchivedDocID = I.ArchivedDocID";
            filter += " AND " + GetBaseSingleConditionFilter(conditionRow);
            advancedCommandText += filter;

			advancedCommandText += archivedDocGroupBy;

			return advancedCommandText;
        }

		

		//--------------------------------------------------------------------------------
		private string GetArchivedDocContentCmdText()
		{
			if (string.IsNullOrEmpty(fea.FreeTag))
				return string.Empty;
			List<string> values = TokenizeSearchText(fea.FreeTag);

			string contentCommandText = string.Empty;
			string tokenParam = string.Empty;
			int i = 0;
			string filter = string.Empty;

			//se l'utente ha specificato un tipo di estensione prima verifico se l'estensione ha un IFilter o meno
			//se ha un IFilter allora faccio la query solo nella tabella DMS_ArchivedDocContent altrimenti nella DMS_ArchivedDocTextContent
			if (fea.DocExtensionType == CommonStrings.AllExtensions || DMSOrchestrator.FullTextFilterManager.IsIFilterDocType(fea.DocExtensionType))
			{
				contentCommandText = archivedDocBaseSelected;

				//prima faccio l'estrazione dalla tabella DMS_ArchivedDocContent
				contentCommandText += ", DMS_ArchivedDocContent C";

				contentCommandText += " WHERE ";

				filter = GetArchivedDocBaseFilter();
				if (!string.IsNullOrEmpty(filter))
					filter += " AND ";
				filter += " D.ArchivedDocID = C.ArchivedDocID";

				contentCommandText += filter + " AND C.OCRProcess = 1";
				foreach (string freeTagToken in values)
				{
					i++;
					tokenParam = "@ContentParam" + i.ToString();
					contentCommandText += string.Format(@" AND CONTAINS(C.BinaryContent, {0})", tokenParam);
					if (!sqlCommand.Parameters.Contains(tokenParam))
						sqlCommand.Parameters.Add(new SqlParameter(tokenParam, "\"*" + freeTagToken + "*\""));
				}

				contentCommandText += archivedDocGroupBy;				
			}

			if (fea.DocExtensionType == CommonStrings.AllExtensions || !DMSOrchestrator.FullTextFilterManager.IsIFilterDocType(fea.DocExtensionType))
			{
				if (fea.DocExtensionType == CommonStrings.AllExtensions)
					contentCommandText += " UNION ";

				contentCommandText += archivedDocBaseSelected;
				//poi faccio l'estrazione dalla tabella DMS_ArchivedDocTextContent per i file pdf
				contentCommandText += ", DMS_ArchivedDocContent C, DMS_ArchivedDocTextContent T";

				contentCommandText += " WHERE ";
				filter = string.Empty;
				filter += GetArchivedDocBaseFilter();
				if (!string.IsNullOrEmpty(filter))
					filter += " AND ";
				filter += " (D.ArchivedDocID = C.ArchivedDocID AND C.ArchivedDocID = T.ArchivedDocID)";

				contentCommandText += filter + " AND C.OCRProcess = 1";
				i = 0;
				foreach (string freeTagToken in values)
				{
					i++;
					tokenParam = "@ContentParam" + i.ToString();
					contentCommandText += string.Format(@" AND CONTAINS(T.TextContent, {0})", tokenParam);
					if (!sqlCommand.Parameters.Contains(tokenParam))
						sqlCommand.Parameters.Add(new SqlParameter(tokenParam, "\"*" + freeTagToken + "*\""));
				}

				contentCommandText += archivedDocGroupBy;		
			}

			return contentCommandText;
		}


		//-------------------------------------------------------------------------------
		private string GetArchivedDocCmdText()
		{
			string archivedDocCmdText = string.Empty;

			if (string.IsNullOrEmpty(fea.FreeTag) && (fea.SearchFieldsConditionsDT == null || fea.SearchFieldsConditionsDT.Rows.Count == 0))
			{
				archivedDocCmdText = archivedDocBaseSelected;
				archivedDocCmdText += " WHERE ";
				archivedDocCmdText += GetArchivedDocBaseFilter();
				archivedDocCmdText += archivedDocGroupBy;

				//se c'è la security attiva allora devo estrarre solo i documenti senza allegati
				if (DMSOrchestrator.SecurityEnabled && !string.IsNullOrEmpty(docNamespacesToProtect))
					archivedDocCmdText += " HAVING count(A.ArchivedDocID) = 0";
				 
				return archivedDocCmdText;
			}
			else
			{
				if (!string.IsNullOrEmpty(fea.FreeTag))
				{

					if (fea.SearchLocation != SearchLocation.Content)
					{
						List<string> values = TokenizeSearchText(fea.FreeTag);
						archivedDocCmdText += "( ";
						string freeText = string.Empty;
						int i = 0;
						string tokenParam = string.Empty;

						foreach (string token in values)
						{
							if (!string.IsNullOrEmpty(freeText))
								freeText += " INTERSECT ";
							{
								freeText += "( ";
								i++;
								tokenParam = "@Param" + i.ToString();
								freeText += GetArchivedDocSingleCmdText(token, tokenParam);
								freeText += " )";
							}
						}

						archivedDocCmdText += freeText;
						archivedDocCmdText += " )";

					}

					if ((fea.SearchLocation == SearchLocation.All || (fea.SearchLocation & SearchLocation.Content) == SearchLocation.Content))
					{
						//se ho scelto solo il contenuto del documento non devo fare la UNION
						if (fea.SearchLocation != SearchLocation.Content)
							archivedDocCmdText += " UNION ";
						archivedDocCmdText += "( ";
						archivedDocCmdText += GetArchivedDocContentCmdText();
						archivedDocCmdText += " )";
					}

					if (!string.IsNullOrEmpty(archivedDocCmdText))
						archivedDocCmdText = "( " + archivedDocCmdText + " )";
				}

				if (fea.SearchFieldsConditionsDT != null && fea.SearchFieldsConditionsDT.Rows.Count > 0)
				{
					string conditionText = string.Empty;
					foreach (DataRow row in fea.SearchFieldsConditionsDT.Rows)
					{
						if (!fea.SearchFieldsConditionsDT.ValidRow(row))
							continue;

						if (!string.IsNullOrEmpty(conditionText))
							conditionText += " INTERSECT ";
						{
							conditionText += "( ";
							conditionText += GetArchivedDocAdvancedCmdText(row);
							conditionText += " )";
						}
					}

					if (!string.IsNullOrEmpty(conditionText))
					{
						if (!string.IsNullOrEmpty(archivedDocCmdText))
							archivedDocCmdText += " INTERSECT ";

						archivedDocCmdText += "( " + conditionText + " )"; ;
					}
				}
			}

			if (!string.IsNullOrEmpty(archivedDocCmdText))
				archivedDocCmdText = "( " + archivedDocCmdText + " )";

			return archivedDocCmdText;
		}

		//--------------------------------------------------------------------------------
		private string GetAttachmentBaseFilter()
        {
            string attBaseFilter = string.Empty;
            attBaseFilter = "D.ArchivedDocID = A.ArchivedDocID";
            //date range
            if (fea.StartDate != DateTime.MinValue && fea.EndDate != DateTime.MaxValue)
            {
                attBaseFilter += string.Format(@" AND ( ((D.TBModified BETWEEN @startDate AND @endDate ) OR (D.TBCreated BETWEEN @startDate AND @endDate)) OR ((A.TBModified BETWEEN @startDate AND @endDate ) OR (A.TBCreated BETWEEN @startDate AND @endDate)) )");
                if (!sqlCommand.Parameters.Contains("@startDate"))
                {
                    sqlCommand.Parameters.Add(new SqlParameter("@startDate", fea.StartDate));
                    sqlCommand.Parameters.Add(new SqlParameter("@endDate", fea.EndDate));
                }
            }

			
		
			string commonBaseFilter = GetCommonBaseFilter();
            if (!string.IsNullOrEmpty(commonBaseFilter))
            {
                attBaseFilter += " AND ";
                attBaseFilter += commonBaseFilter;
            }

			//se devo controllare per la security
			if (DMSOrchestrator.SecurityEnabled && !string.IsNullOrEmpty(docNamespacesToProtect))
			{
				attBaseFilter += " AND ";
				attBaseFilter +=  string.Format("(A.ErpDocumentID = E.ErpDocumentID  AND A.ErpDocumentID = E.ErpDocumentID AND E.DocNamespace NOT IN({0})) ", docNamespacesToProtect);
			}
	
            return attBaseFilter;
        }

		//--------------------------------------------------------------------------------
		private string GetAttachmentSingleCmdText(string freeTagToken, string freeTagTokenParam)
		{
			string attachmentCommandText = string.Empty;

            attachmentCommandText = attachmentBaseSelected;

            if (!string.IsNullOrEmpty(freeTagToken))
                //aggiungo le tabelle relative agli indici di ricerca
                attachmentCommandText += ", DMS_AttachmentSearchIndexes I, DMS_SearchFieldIndexes S";

			if (DMSOrchestrator.SecurityEnabled && !string.IsNullOrEmpty(docNamespacesToProtect))
				attachmentCommandText += ", DMS_ErpDocument E";


			attachmentCommandText += " WHERE ";
            string filter = GetAttachmentBaseFilter();
            
            if (!string.IsNullOrEmpty(freeTagToken))
            {
                if (!string.IsNullOrEmpty(filter))
                    filter += "AND ";
				filter += GetCommonFreeTagTokenFilter(freeTagToken, freeTagTokenParam, true);
			}

            attachmentCommandText += filter;

            return attachmentCommandText;
        }

        //--------------------------------------------------------------------------------
        private string GetAttachmentAdvancedCmdText(DataRow conditionRow)
        {
            if (conditionRow == null)
                return string.Empty;

            string advancedCommandText = string.Empty;
            advancedCommandText = attachmentBaseSelected;
            advancedCommandText += ", DMS_AttachmentSearchIndexes I";
			
			//Security
			if (DMSOrchestrator.SecurityEnabled && !string.IsNullOrEmpty(docNamespacesToProtect))
				advancedCommandText += ", DMS_ErpDocument E";

			advancedCommandText += " WHERE ";

            //WHERE clause
            string filter = GetAttachmentBaseFilter();
            if (!string.IsNullOrEmpty(filter))
                filter += " AND ";
            filter += "A.AttachmentID = I.AttachmentID";
            filter += " AND " + GetBaseSingleConditionFilter(conditionRow);
            advancedCommandText += filter;

            return advancedCommandText;
        }

		//--------------------------------------------------------------------------------
		private string GetAttachmentContentCmdText()
		{
			if(string.IsNullOrEmpty(fea.FreeTag))
				return string.Empty;
			List<string> values = TokenizeSearchText(fea.FreeTag);

			string contentCommandText = string.Empty;
			string tokenParam = string.Empty;
			int i = 0;
			string filter = string.Empty;

			//se l'utente ha specificato un tipo di estensione prima verifico se l'estensione ha un IFilter o meno
			//se ha un IFilter allora faccio la query solo nella tabella DMS_ArchivedDocContent altrimenti nella DMS_ArchivedDocTextContent
			if (fea.DocExtensionType == CommonStrings.AllExtensions || DMSOrchestrator.FullTextFilterManager.IsIFilterDocType(fea.DocExtensionType))
			{
				contentCommandText = attachmentBaseSelected;

				//prima faccio l'estrazione dalla tabella DMS_ArchivedDocContent
				contentCommandText += ", DMS_ArchivedDocContent C";

				//Security
				if (DMSOrchestrator.SecurityEnabled && !string.IsNullOrEmpty(docNamespacesToProtect))
					contentCommandText += ", DMS_ErpDocument E";

				contentCommandText += " WHERE ";

				filter = GetAttachmentBaseFilter();

				if (!string.IsNullOrEmpty(filter))
					filter += " AND ";
				filter += " D.ArchivedDocID = C.ArchivedDocID";

				contentCommandText += filter + " AND C.OCRProcess = 1";

				foreach (string freeTagToken in values)
				{
					i++;
					tokenParam = "@ContentParam" + i.ToString();
					contentCommandText += string.Format(@" AND CONTAINS(C.BinaryContent, {0})", tokenParam);
					if (!sqlCommand.Parameters.Contains(tokenParam))
						sqlCommand.Parameters.Add(new SqlParameter(tokenParam, "\"*" + freeTagToken + "*\""));
				}
			}

			if (fea.DocExtensionType == CommonStrings.AllExtensions || !DMSOrchestrator.FullTextFilterManager.IsIFilterDocType(fea.DocExtensionType))
			{
				if (fea.DocExtensionType == CommonStrings.AllExtensions)
					contentCommandText += " UNION ";

				contentCommandText += attachmentBaseSelected;
				//poi faccio l'estrazione dalla tabella DMS_ArchivedDocTextContent per i file pdf
				contentCommandText += ", DMS_ArchivedDocContent C, DMS_ArchivedDocTextContent T";

				//Security
				if (DMSOrchestrator.SecurityEnabled && !string.IsNullOrEmpty(docNamespacesToProtect))
					contentCommandText += ", DMS_ErpDocument E";

				contentCommandText += " WHERE ";
				filter = string.Empty;
				filter += GetAttachmentBaseFilter();
				if (!string.IsNullOrEmpty(filter))
					filter += " AND ";
				filter += " (D.ArchivedDocID = C.ArchivedDocID AND C.ArchivedDocID = T.ArchivedDocID)";

				contentCommandText += filter + " AND C.OCRProcess = 1";
				i = 0;
				foreach (string freeTagToken in values)
				{
					i++;
					tokenParam = "@ContentParam" + i.ToString();
					contentCommandText += string.Format(@" AND CONTAINS(T.TextContent, {0})", tokenParam);
					if (!sqlCommand.Parameters.Contains(tokenParam))
						sqlCommand.Parameters.Add(new SqlParameter(tokenParam, "\"*" + freeTagToken + "*\""));
				}
			}

			return contentCommandText;
		}

		//-------------------------------------------------------------------------------
		private string GetAttachmentCmdText()
        {
			string attachmentCmdText = string.Empty;
			
			if ((string.IsNullOrEmpty(fea.FreeTag)) && (fea.SearchFieldsConditionsDT == null || fea.SearchFieldsConditionsDT.Rows.Count == 0))
			{
				attachmentCmdText = attachmentBaseSelected;
				//Security
				if (DMSOrchestrator.SecurityEnabled && !string.IsNullOrEmpty(docNamespacesToProtect))
					attachmentCmdText += ", DMS_ErpDocument E";

				attachmentCmdText += " WHERE ";

				attachmentCmdText += GetAttachmentBaseFilter();
				return attachmentCmdText;
			}
			else
			{
				if (!string.IsNullOrEmpty(fea.FreeTag))
				{
					if (fea.SearchLocation != SearchLocation.Content && fea.SearchLocation != SearchLocation.Barcode)
					{
						List<string> values = TokenizeSearchText(fea.FreeTag);
						string freeText = string.Empty;
						int i = 0;
						string tokenParam = string.Empty;
						attachmentCmdText += "( ";
						foreach (string token in values)
						{
							if (!string.IsNullOrEmpty(freeText))
								freeText += " INTERSECT ";
							{
								freeText += "( ";
								i++;
								tokenParam = "@Param" + i.ToString();
								freeText += GetAttachmentSingleCmdText(token, tokenParam);
								freeText += " )";
							}
						}
						attachmentCmdText += freeText;
						attachmentCmdText += " )";

					}

					if (!string.IsNullOrEmpty(attachmentCmdText))
						attachmentCmdText = "( " + attachmentCmdText + " )";
				}

				if (fea.SearchFieldsConditionsDT != null && fea.SearchFieldsConditionsDT.Rows.Count > 0)
				{
					string conditionText = string.Empty;
					foreach (DataRow row in fea.SearchFieldsConditionsDT.Rows)
					{
						if (!fea.SearchFieldsConditionsDT.ValidRow(row))
							continue;

						if (!string.IsNullOrEmpty(conditionText))
							conditionText += " INTERSECT ";
						{
							conditionText += "( ";
							conditionText += GetAttachmentAdvancedCmdText(row);
							conditionText += " )";
						}
					}

					if (!string.IsNullOrEmpty(conditionText))
					{
						if (!string.IsNullOrEmpty(attachmentCmdText))
							attachmentCmdText += " INTERSECT ";

						attachmentCmdText += "( " + conditionText + " )"; ;
					}
				}
			}

			if (!string.IsNullOrEmpty(attachmentCmdText))
				attachmentCmdText = "( " + attachmentCmdText + " )";

			return attachmentCmdText;
		}
		
        //--------------------------------------------------------------------------------
        public ArchivedDocDataTable GetSqlArchivedDocuments()
        {
            ArchivedDocDataTable searchResult = new ArchivedDocDataTable();
            if (fea == null)
                return searchResult;

            SqlConnection connection = null;
            SqlDataReader reader = null;
            try
            {
                connection = new SqlConnection(DMSOrchestrator.DMSConnectionString);
                connection.Open();
                if (sqlCommand != null)
                    sqlCommand.Dispose();

                sqlCommand = new SqlCommand();
                sqlCommand.Connection = connection;

				string archivedDocCmdText = string.Empty;
                string attachmentCmdText = string.Empty;
                string completedCmdTex = string.Empty;

				//anche se l'utente ha scelto una specifica collection vado a fare la query sui documenti archiviati poichè alcune chiavi di ricerca
				// (vedi BarcodeTag, Description, FreeTag sono contenute anche nella tabella DMS_ArchivedDocSearchIndexes.
				//Nel caso di collection i documenti archiviati sono filtarti per la collection dei propri allegati

				docNamespacesToProtect = string.Empty;

				//se la query è su tutto il repository devo verificare se attiva o meno 
				//la security e nel caso scartare gli allegati che si riferiscono ai documenti sotto protezione 
				if (fea.CollectionID < 0 && DMSOrchestrator.SecurityEnabled)
				{
					ERPDocumentsGrant docsGrant = LoadERPDocumentsGrant();
					foreach (DataRow docNamespace in docsGrant.Rows)
					{
						if (!(bool)docNamespace[CommonStrings.CanShowAttachments])
						{
							if (!string.IsNullOrEmpty(docNamespacesToProtect))
								docNamespacesToProtect += ", ";
							docNamespacesToProtect += string.Format(@"'{0}'", docNamespace[CommonStrings.DocNamespace]);
						}
					}
				}

				archivedDocCmdText = GetArchivedDocCmdText();
				attachmentCmdText = GetAttachmentCmdText();
                                        
                if (!string.IsNullOrEmpty(archivedDocCmdText))
                    completedCmdTex += archivedDocCmdText;

                if (!string.IsNullOrEmpty(attachmentCmdText))
                {
                    if (!string.IsNullOrEmpty(completedCmdTex))
                        completedCmdTex += " UNION ";
                    completedCmdTex += attachmentCmdText;
                }
               
                if (!string.IsNullOrEmpty(completedCmdTex))
                {
                    sqlCommand.CommandText = (fea.TopDocsNumber > 0) ? string.Format("SELECT Distinct TOP {0} * FROM ( ", fea.TopDocsNumber.ToString()) : "SELECT Distinct * FROM ( ";
                    sqlCommand.CommandText += completedCmdTex + " ) AS ResultTable ORDER BY TBModified DESC";

                    reader = sqlCommand.ExecuteReader();

                    while (reader.Read())
                    {
                        DataRow newRow = searchResult.NewRow();
                        newRow[CommonStrings.ArchivedDocID] = reader[CommonStrings.ArchivedDocID];
                        newRow[CommonStrings.Name] = reader[CommonStrings.Name];
                        newRow[CommonStrings.TBModified] = reader[CommonStrings.TBModified];
						newRow[CommonStrings.TBCreated] = reader[CommonStrings.TBCreated];
                       
                        MWorker worker = DMSOrchestrator.WorkersTable.GetWorker((int)reader[CommonStrings.TBModifiedID]);
                        newRow[CommonStrings.WorkerName] = (worker != null) ? worker.Name + " " + worker.LastName : reader[CommonStrings.TBModifiedID].ToString();
                        newRow[CommonStrings.ModifierID] = reader[CommonStrings.ModifierID];
                        newRow[CommonStrings.StorageType] = (reader.IsDBNull(reader.GetOrdinal(CommonStrings.StorageType))) ? 0 : (int)reader["StorageType"];

                         newRow[CommonStrings.Description] = reader[CommonStrings.Description].ToString();
                        newRow[CommonStrings.IsWoormReport] = reader[CommonStrings.IsWoormReport].ToString();
						newRow[CommonStrings.Attached] = reader["hasAttach"] != DBNull.Value ? (int)reader["hasAttach"] == 1 : false;

						searchResult.Rows.Add(newRow);
                        newRow[CommonStrings.AttachmentInfo] = null; //on demand. Lo carico solo se necessario
                    }

                    reader.Close();
                    sqlCommand.Dispose();
                }
            }
            catch (SqlException e)
            {
                SetMessage(Strings.ErrorLoadingArchivedDoc, e, "GetArchivedDocuments");
            }
            finally
            {
                if (reader != null)
                    reader.Close();

                if (sqlCommand != null)
                    sqlCommand.Dispose();

                if (connection != null)
                {
                    if (connection.State == ConnectionState.Open)
                        connection.Close();
                    connection.Dispose();
                }
            }

            return searchResult;
        }      

    }
}
