using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using Microarea.EasyAttachment.Components;
using Microarea.EasyAttachment.Core;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.EasyAttachment.BusinessLogic
{
	///<summary>
	/// DocumentManagement class
	///</summary>
	//================================================================================
	public class ArchiveManager : BaseManager
	{
		// private variables
		protected DMSModelDataContext dc = null;

		protected SearchManager searchManager = null;
        protected CategoryManager categoryManager = null;
        protected BarcodeManager barcodeManager = null;
        protected SOSManager sosManager = null;

        
        protected DuplicateManager duplicateManager = null;
        protected DuplicateDocumentAction existingAction = DuplicateDocumentAction.Cancel;
        protected BookmarksDataTable defaultBookmarksDT = null;	
		
		//properties
		public BookmarksDataTable DefaultBookmarksDT { get { return defaultBookmarksDT; } }

		// Events
		public event EventHandler<AttachmentInfoEventArgs> ArchiveCompleted;
        public event EventHandler<CollectionEventArgs> UpdateCollectionCompleted;

        public event EventHandler<AttachmentEventArgs> AttachmentDeleted;

        /// <summary>
        /// Document Management constructor
        /// </summary>
        //---------------------------------------------------------------------
        public ArchiveManager(DMSOrchestrator dmsOrchestrator)
		{
			DMSOrchestrator = dmsOrchestrator;
			ManagerName = "ArchiveManager";

			/* <add name="Microarea.EasyAttachment.Properties.Settings.MDMS_SAMPLEConnectionString"
		   connectionString="Data Source=USR-BAUZONEANN1\SQLEXPRESS;Initial Catalog=MDMS_SAMPLE;Integrated Security=True"
		   providerName="System.Data.SqlClient" />*/

			dc = DMSOrchestrator.DataContext;
		
			searchManager = dmsOrchestrator.SearchManager;
			categoryManager = dmsOrchestrator.CategoryManager;
			barcodeManager = dmsOrchestrator.BarcodeManager;
            sosManager = dmsOrchestrator.SosManager;


			duplicateManager = new DuplicateManager(DMSOrchestrator);
		}
		
		/// <summary>
		/// Restitusce gli eventuali template di memorizzazione assegnati alla singola collection
		/// </summary>
		/// <param name="collection">nome della collection</param>
		/// <returns>lista degli eventuali template presenti nella collection</returns>
		//---------------------------------------------------------------------
		public List<string> GetTemplates()
		{
			var templates = from coll in dc.DMS_Collections
							where coll.Name == DMSOrchestrator.CollectionName
							select coll.TemplateName;
			return templates.ToList();
		}



        ///<summary>
        /// Delete del DMS_Attachment nel DB
        ///</summary>
        //---------------------------------------------------------------------
        private bool DeleteDMS_Attachment(int attachmentId)
        {
            DMS_Attachment attachment = searchManager.GetAttachment(attachmentId);
            if (attachment == null)
                return false;

            try
            {
                string lockMsg = string.Empty;
                if (DMSOrchestrator.LockManager.LockRecord(attachment, DMSOrchestrator.LockContext, ref lockMsg))
                {
                    if (DMSOrchestrator.SosConnectorEnabled && !sosManager.DeleteSosDocument(attachmentId))
                    {
                        DMSOrchestrator.LockManager.UnlockRecord(attachment, DMSOrchestrator.LockContext);
                        SetMessage(Strings.ErrorSosConnectorProcessAttachment, null, "DeleteAttachment");
                        return false;
                    }

                    searchManager.DeleteAttachmentIndexes(attachmentId);
                    dc.DMS_Attachments.DeleteOnSubmit(attachment);
                    dc.SubmitChanges();
                    DMSOrchestrator.LockManager.UnlockRecord(attachment, DMSOrchestrator.LockContext);
                    if (AttachmentDeleted != null)
                    {
                        AttachmentEventArgs arg = new AttachmentEventArgs();
                        arg.AttachmentID = attachmentId;
                        AttachmentDeleted(this, arg);
                    }
                }
                else
                    SetMessage(Strings.ErrorDeletingAttachment, new Exception(lockMsg), "DeleteAttachment");
            }
            catch (Exception e)
            {
                SetMessage(Strings.ErrorDeletingAttachment, e, "DeleteAttachment");
                return false;
            }

            return true;
        }

		
        //---------------------------------------------------------------------        
        public bool DeleteAttachment(int attachmentId, int archivedDocId)
        {
            int archDocId = archivedDocId;
            if (archDocId == -1 && DMSOrchestrator.SettingsManager.UsersSettingState.Options.DeleteOptionsState.DeletingAttachmentAction
                 != DeletingAttachmentAction.KeepArchivedDoc)
            {
                DMS_Attachment attachment = searchManager.GetAttachment(attachmentId);
                if (attachment == null)
                    return true;
                archDocId = attachment.ArchivedDocID;
            }

            bool deleteRes = DeleteDMS_Attachment(attachmentId);
           
            // se nei parametri ho scelto di non mantenere il documento archiviato
            if (deleteRes && DMSOrchestrator.SettingsManager.UsersSettingState.Options.DeleteOptionsState.DeletingAttachmentAction
                != DeletingAttachmentAction.KeepArchivedDoc)
            {
                // controllo prima se posso eliminare il documento archiviato perche' non utilizzato da altri attachment
                if (CanDeleteArchivedDocument(archDocId, attachmentId))
                {
                    switch (DMSOrchestrator.SettingsManager.UsersSettingState.Options.DeleteOptionsState.DeletingAttachmentAction)
                    {
                        case DeletingAttachmentAction.AskBeforeDeleteArchivedDoc:
                            if (!DMSOrchestrator.InUnattendedMode && DiagnosticViewer.ShowQuestion(Strings.DeleteArchivedDoc, Strings.DeleteArchivedDocTitle) == DialogResult.Yes)
                                deleteRes = DeleteArchiveDoc(archDocId);
                            break;

                        case DeletingAttachmentAction.DeleteArchivedDoc:
                            deleteRes = DeleteArchiveDoc(archDocId);
                            break;
                    }
                }
            }
            return deleteRes;
        }

        //---------------------------------------------------------------------
        bool DeleteAttachmentList(List<DMS_Attachment> attachments)
        {
            bool deleteRes = false; ;
            bool ask = false;
            

            if (DMSOrchestrator.SettingsManager.UsersSettingState.Options.DeleteOptionsState.DeletingAttachmentAction == DeletingAttachmentAction.AskBeforeDeleteArchivedDoc)
            {
                foreach (DMS_Attachment att in attachments)
                {
                    // controllo prima se posso eliminare il documento archiviato perche' non utilizzato da altri attachment
                    //se esiste almeno un documento con questa tipologia allora devo chiedere all'utente (visto che nelle 
                    //option ho AskBeforeDeleteArchivedDoc)
                    if (CanDeleteArchivedDocument(att.ArchivedDocID, att.AttachmentID))
                    {
                        ask = true;
                        break;
                    }
                }
            }

            bool removeArchivedDoc = false;
            switch (DMSOrchestrator.SettingsManager.UsersSettingState.Options.DeleteOptionsState.DeletingAttachmentAction)
            {
                case DeletingAttachmentAction.DeleteArchivedDoc:
                    removeArchivedDoc = true;
                    break;
                case DeletingAttachmentAction.AskBeforeDeleteArchivedDoc:
                    removeArchivedDoc = (DMSOrchestrator.InUnattendedMode) ? false : (ask) ? DiagnosticViewer.ShowQuestion(Strings.DeleteArchivedDoc, Strings.DeleteArchivedDocTitle) == DialogResult.Yes : false;
                    break;
                default:
                    break;
            }

            int attachmentId = 0;
            int archivedDocId = 0;
            foreach (DMS_Attachment att in attachments)
            {
                attachmentId = att.AttachmentID;
                archivedDocId = att.ArchivedDocID;

                deleteRes = DeleteDMS_Attachment(attachmentId) && deleteRes;
                if (removeArchivedDoc && CanDeleteArchivedDocument(archivedDocId, attachmentId))
                    deleteRes = DeleteArchiveDoc(archivedDocId) && deleteRes;
            }
            
            return deleteRes;

        }


        //remove only attachments
        //---------------------------------------------------------------------
        public void DeleteAttachments(int erpDocumentID)
        {
             var attachments = from a in dc.DMS_Attachments
                              where a.DMS_ErpDocument.ErpDocumentID == erpDocumentID
                              select a;
             try
             {
                 // elimino tutti gli attachments esistenti
                 if (attachments != null && attachments.Any())
                    DeleteAttachmentList(attachments.ToList());
             }
             catch (Exception ex)
             {
                 throw (ex);
             }
        }

     
        //---------------------------------------------------------------------
        public void DeleteErpDocument(int erpDocumentID)
        {
            if (erpDocumentID <= 0)
                return;

            try
            {
                // elimino tutti gli attachments esistenti
                DeleteAttachments(erpDocumentID);

                // elimino i record anche dalle tabelle DMS_ErpDocument e DMS_ErpDocBarcodes
                var barcodes = from bc in dc.DMS_ErpDocBarcodes
                               where bc.ErpDocumentID == erpDocumentID
                               select bc;

                if (barcodes != null && barcodes.Any())
                {
                    dc.DMS_ErpDocBarcodes.DeleteAllOnSubmit(barcodes);
                    dc.SubmitChanges();
                }

                var erpDocs = from doc in dc.DMS_ErpDocuments
                              where doc.ErpDocumentID == erpDocumentID
                              select doc;

                DMS_ErpDocument erpDoc = null;

                if (erpDocs != null && erpDocs.Any())
                    erpDoc = (DMS_ErpDocument)erpDocs.Single();

                dc.DMS_ErpDocuments.DeleteOnSubmit(erpDoc);
                dc.SubmitChanges();
            }
            catch (Exception ex)
            {
                throw (ex);
            }          

        }

        //---------------------------------------------------------------------
        private BookmarksDataTable GetRepositoryCollectionFields(DMS_Collection collection)
        {
            BookmarksDataTable dataTable = new BookmarksDataTable();
            IQueryable<DMS_CollectionsField> fields = null;

            try
            {
                 fields = from coll in dc.DMS_CollectionsFields
                            where coll.CollectionID == collection.CollectionID
                            && coll.Disabled == false
                            select coll;

                
                {
                    dataTable.Fill(fields);                    
                    SyncronizeFieldsValue(dataTable);
                    dataTable.AcceptChanges();
                }
            }
            catch (Exception e)
            {
                throw (e);
            }
            return dataTable;
        }


        /// <summary>
		/// used to syncronize the datafield value with the ERP current browsed document
		/// </summary>
		//---------------------------------------------------------------------
		virtual public void SyncronizeFieldsValue(BookmarksDataTable fields)
		{
			try
			{
				if (fields == null)
					return;

				for (int i = 0; i < fields.Rows.Count; i++)
				{
					DataRow row = fields.Rows[i];
					// se non e' specificato un nome nel grid non eseguo la sincronizzazione del field
                    if (
                            row != null &&
                            !string.IsNullOrWhiteSpace(row[CommonStrings.Name].ToString())
                        )
                    {
                        string fieldName = row[CommonStrings.Name].ToString();
                        if (((FieldGroup)row[CommonStrings.FieldGroup]) == FieldGroup.Category)
                        {
                            ((FieldData)row[CommonStrings.FieldData]).StringValue = categoryManager.GetCategoryDefaultValue(row[CommonStrings.Name].ToString());
                            row[CommonStrings.ValueSet] = categoryManager.GetCategoryValues(row[CommonStrings.Name].ToString());
                            row[CommonStrings.FormattedValue] = ((FieldData)row[CommonStrings.FieldData]).FormattedValue;
                        }
                    }
				}
				fields.AcceptChanges();
			}
			catch (Exception e)
			{
				SetMessage(Strings.ErrorSynchronizingField, e, "SyncronizeFieldsValue");
			}
		}


        /// <summary>
        /// Data una collection ed un template restituisce il nome dei campi utilizzabili per creare gli indici di ricerca
        /// </summary>
        //---------------------------------------------------------------------
        public BookmarksDataTable GetCollectionFields(int collectionID, BookmarksDataTable.DisableFilter disabledFilter, DMSDocOrchestrator dmsDocOrch = null, bool syncFields = true)
        {
            BookmarksDataTable dataTable = null;
            try
            {
                IQueryable<DMS_CollectionsField> fields = null;

                if (disabledFilter == BookmarksDataTable.DisableFilter.Both)
                {
                    fields = from coll in dc.DMS_CollectionsFields
                             where coll.CollectionID == collectionID
                             select coll;
                }
                else
                {
                    bool disabled = disabledFilter == BookmarksDataTable.DisableFilter.OnlyDisable;
                    fields = from coll in dc.DMS_CollectionsFields
                             where coll.CollectionID == collectionID
                             && coll.Disabled == disabled
                             select coll;
                }

                dataTable = new BookmarksDataTable();
                dataTable.Fill(fields, dmsDocOrch);
                if (syncFields)
                    SyncronizeFieldsValue(dataTable);
            }
            catch (Exception e)
            {
                SetMessage(string.Format(Strings.ErrorLoadingCollectionFields, collectionID.ToString()), e, "GetCollectionFields");
                return null;
            }

            EnumerableRowCollection<DataRow> enumDataTable = dataTable.AsEnumerable();
            var orderDataTable = from resRow in enumDataTable
                                 orderby resRow[CommonStrings.FieldGroup], resRow[CommonStrings.FieldDescription]
                                 select resRow;

            BookmarksDataTable newCF = new BookmarksDataTable();
            orderDataTable.CopyToDataTable(newCF, LoadOption.OverwriteChanges);
            return newCF;
        }
		/// <summary>
		/// dato un documentID, attachmentID e la collezione utilizzata per l'archiviazione, restituisce i valori
		/// delle chiavi di ricerca
		/// </summary>
		//---------------------------------------------------------------------
		public void GetBookmarksValues(int collectionID, ref AttachmentInfo attachmentInfo)
		{
            if (attachmentInfo.BookmarksDT == null)
                attachmentInfo.BookmarksDT = GetCollectionFields(collectionID, BookmarksDataTable.DisableFilter.Both);
			searchManager.GetBookmarksValues(ref attachmentInfo);
            attachmentInfo.BookmarksDT.AcceptChanges();
        }		    

		
		//---------------------------------------------------------------------
		public bool UpdateArchivedDoc(ref AttachmentInfo attachmentInfo, string newDescription, string newTags)
		{
			if (attachmentInfo == null || attachmentInfo.ArchivedDocId < 0)
				return false;

			//check if something have been changed
			int archivedDocId = attachmentInfo.ArchivedDocId;

			//check if it exists the attachment
			var var = (from att in dc.DMS_ArchivedDocuments
					   where att.ArchivedDocID == archivedDocId
					   select att);

			DMS_ArchivedDocument archived = null;
			try
			{
				archived = (var != null && var.Any()) ? (DMS_ArchivedDocument)var.Single() : null;

				if (archived == null)
					return false;

				string lockMsg = string.Empty;
				if (DMSOrchestrator.LockManager.LockRecord(archived, DMSOrchestrator.LockContext, ref lockMsg))
				{
					// ith bookmarks are changed I have to update the search indexes and collection template	
					if (
							string.Compare(attachmentInfo.Tags, newTags, StringComparison.InvariantCultureIgnoreCase) != 0 ||
							string.Compare(attachmentInfo.Description, newDescription, StringComparison.InvariantCultureIgnoreCase) != 0 ||				
							attachmentInfo.BookmarksDT.GetChanges() != null
						)
					{
						UpdateSearchIndexes(attachmentInfo, newTags, newDescription);
                        attachmentInfo.Tags = newTags;
                        attachmentInfo.Description = newDescription;

						if (attachmentInfo.BookmarksDT.GetChanges() != null)
						{
							SaveStandardTemplate(DMSOrchestrator.RepositoryCollection, attachmentInfo.BookmarksDT);
							searchManager.GetBookmarksValues(ref attachmentInfo);
                            attachmentInfo.BookmarksDT.AcceptChanges();
						}
						//different description:
						if (string.Compare(archived.Description, newDescription, StringComparison.InvariantCultureIgnoreCase) != 0)
							archived.Description = newDescription;
					}

                    archived.TBModified = dc.GetDate();
                    dc.SubmitChanges();

                    DMSOrchestrator.LockManager.UnlockRecord(archived, DMSOrchestrator.LockContext);
				}
				else
				{
					SetMessage(string.Format(Strings.ErrorUpdatingArchivedDoc, attachmentInfo.ArchivedDocId.ToString()), new Exception(lockMsg), "UpdateArchivedDoc");
					return false;
				}
			}
			catch (Exception e)
			{
				SetMessage(string.Format(Strings.ErrorUpdatingArchivedDoc, attachmentInfo.ArchivedDocId.ToString()), e, "UpdateArchivedDoc");
				return false;
			}
			return true;
		}

		//---------------------------------------------------------------------
		public void UpdateSearchIndexes(AttachmentInfo attachmentInfo, string tags, string description)
		{
			try
			{
				searchManager.UpdateIndexes(attachmentInfo, tags, description);
			}
			catch (Exception e)
			{
				throw e;
			}
		}

		//---------------------------------------------------------------------
		public void LoadAttachment(ref AttachmentInfo attachmentInfo)
		{
			if (attachmentInfo.OnDemandInfoLoaded)
				return;
			
			int attachmentID = attachmentInfo.AttachmentId;
			//check if it exists the attachment
			var att = (from a in dc.DMS_Attachments
						where a.AttachmentID == attachmentID
						select a);

			DMS_Attachment attachment = (att != null && att.Any()) ? (DMS_Attachment)att.Single() : null;

			if (attachment != null)
			{
				attachmentInfo.Description = attachment.Description;
				GetBookmarksValues(attachment.CollectionID, ref attachmentInfo);
				// todo caricare anche il template dell'attachment + ocr field
			}
			attachmentInfo.OnDemandInfoLoaded = true;
		}

		///Per BrainBusiness
		//---------------------------------------------------------------------
		public void LoadWfInfo(ref AttachmentInfo attachmentInfo)
		{
			if (attachmentInfo == null) return;
			int attachmentID = attachmentInfo.AttachmentId;

			var info = (from a in dc.DMS_WFAttachments
						where a.AttachmentID == attachmentID && a.WorkerID == DMSOrchestrator.WorkerId
						select a);

			DMS_WFAttachments wfInfo = (info != null && info.Any()) ? (DMS_WFAttachments)info.Single() : null;
			if (wfInfo != null)
			{
				//altrimenti non vede quando da brain business approvo o rifiuto
				dc.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, info);

				attachmentInfo.WFRequestDate = wfInfo.RequestDate;
				attachmentInfo.WFApprovalDate = (wfInfo.ApprovalDate == null ? DateTime.MinValue : (DateTime)wfInfo.ApprovalDate); //todo sistemare il tipo
				attachmentInfo.WFApproverId = (wfInfo.ApproverID == null ? 0 : (int)wfInfo.ApproverID);//todo sistemare il tipo
				attachmentInfo.WFRequestComment = wfInfo.RequestComments;
				attachmentInfo.WFApprovalStatus = wfInfo.ApprovalStatus;
				attachmentInfo.WFApprovalComment = wfInfo.ApprovalComments;
			}
		}

		///Per BrainBusiness
		//---------------------------------------------------------------------
		public void InsertMagoUserIntoBrainBusiness()
		{

		}

		//---------------------------------------------------------------------
		public void LoadArchiveDoc(ref AttachmentInfo attachmentInfo)
		{
			if (attachmentInfo.OnDemandInfoLoaded)
				return;

			int archivedDocId = attachmentInfo.ArchivedDocId;

			//check if it exists the attachment
			var att = (from a in dc.DMS_ArchivedDocuments
					   where a.ArchivedDocID == archivedDocId
					   select a);

			DMS_ArchivedDocument archived = (att != null && att.Any()) ? (DMS_ArchivedDocument)att.Single() : null;
			if (archived != null)
				GetBookmarksValues((int)archived.CollectionID, ref attachmentInfo);
		}

		//---------------------------------------------------------------------
		public DMS_Field GetField(string fieldName)
		{
			var field = (from f in dc.DMS_Fields
						 where f.FieldName == fieldName
						 select f);
			return (field != null && field.Any()) ? (DMS_Field)field.Single() : null;
		}

		//---------------------------------------------------------------------
		private void AddNewCollectionField(DataRow row, int collectionID)
		{
			try
			{
				DMS_CollectionsField newCollField = new DMS_CollectionsField();

				DMS_Field field = GetField(row[CommonStrings.Name].ToString());
				if (field == null)
				{
					field = new DMS_Field();
					field.FieldName = row[CommonStrings.Name].ToString();
					field.FieldDescription = row[CommonStrings.FieldDescription].ToString();
					field.ValueType = row[CommonStrings.ValueType].ToString();
					field.IsCategory = false;
				}				
				newCollField.DMS_Field = field;
                newCollField.CollectionID = collectionID;
                //se il campo esiste già (associato ad esempio ad un'altra collection allora FieldName viene valorizzato automaticamente quando effettuo l'assegnazione
                // newCollField.DMS_Field = field; questo è fatto automaticamente da LINQ grazie al fatto che esiste una relazione di FK tra DMS_CollectionsFields e DMS_Field
                //BugFix #21631
                if (string.IsNullOrEmpty(newCollField.FieldName))
                    newCollField.FieldName = row[CommonStrings.Name].ToString();
			
				newCollField.ControlName = row[CommonStrings.ControlName].ToString();
				newCollField.PhysicalName = row[CommonStrings.PhysicalName].ToString();
				newCollField.OCRPosition = row[CommonStrings.OCRPosition].ToString();
				//newCollField.OCRPage =  (row[CommonStrings.OCRPage] == DBNull.Value) ? 0  : (int)(row[CommonStrings.OCRPage]) ;						
				newCollField.FieldGroup = (int)row[CommonStrings.FieldGroup];
				newCollField.ShowAsDescription = (bool)row[CommonStrings.ShowAsDescription];
				//for Repository Collection the field born with Disable property = true so to disable the default template
				newCollField.Disabled = (DMSOrchestrator.RepositoryCollection.CollectionID == collectionID);
				//improvement #5062 : SOSConnector								
				newCollField.SosPosition = (int)row[CommonStrings.SosPosition];
				newCollField.SosMandatory = (row[CommonStrings.SosMandatory] == DBNull.Value) ? false : (bool)row[CommonStrings.SosMandatory];
				newCollField.HKLName =  string.Empty; //(row[CommonStrings.HotKeyLink] == DBNull.Value) ? string.Empty : ((MHotLink)row[CommonStrings.HotKeyLink]).Name;
                newCollField.SosKeyCode = (row[CommonStrings.SosKeyCode] == DBNull.Value || row[CommonStrings.SosKeyCode].ToString() == string.Empty) ? newCollField.FieldName : row[CommonStrings.SosKeyCode].ToString(); //impr. #5305
  
				dc.DMS_CollectionsFields.InsertOnSubmit(newCollField);
			}
			catch (Exception e)
			{
				SetMessage(Strings.ErrorAddingCollectionField, e, "AddNewCollectionField");
			}
		}

		//---------------------------------------------------------------------
		private void ModifyCollectionField(DMS_CollectionsField collField, DataRow row)
		{
			collField.ControlName = row[CommonStrings.ControlName].ToString();
			collField.PhysicalName = row[CommonStrings.PhysicalName].ToString();
			collField.OCRPosition = row[CommonStrings.OCRPosition].ToString();
			// for Repository Collection the field must have Disable property = true so to disable the default template			
			collField.Disabled = (DMSOrchestrator.RepositoryCollection.CollectionID == collField.CollectionID);
			//improvement #5062 : SOSConnector
			collField.SosPosition = (int)row[CommonStrings.SosPosition];
			collField.SosMandatory = (row[CommonStrings.SosMandatory] == DBNull.Value) ? false : (bool)row[CommonStrings.SosMandatory];
			collField.HKLName = (row[CommonStrings.HotKeyLink] == DBNull.Value) ? string.Empty : ((MHotLink)row[CommonStrings.HotKeyLink]).Name;
            collField.SosKeyCode = (row[CommonStrings.SosKeyCode] == DBNull.Value || row[CommonStrings.SosKeyCode].ToString() == string.Empty) ? collField.FieldName : row[CommonStrings.SosKeyCode].ToString();   //impr. #5185           
		}

		/// <summary>
		/// Save the template used to attach the archived document to releated ERP document
		/// </summary>
		//---------------------------------------------------------------------
		public int SaveStandardTemplate(DMS_Collection collection, BookmarksDataTable fields)
		{
			int collectionID = SaveTemplate(collection, fields);

			if (collectionID != -1)
			{
                defaultBookmarksDT = fields;

                if (UpdateCollectionCompleted != null)
                {
                    CollectionEventArgs collEventArg = new CollectionEventArgs();
                    collEventArg.CollectionID = collection.CollectionID;
                    UpdateCollectionCompleted(this, collEventArg);
                }

     //           if (TemplateChanged != null)
					//TemplateChanged(this, new EventArgs());
			}
			return collectionID;
		}
		
		/// <summary>
		/// Called to save a template both for archiving/attaching and for ocr
		/// </summary>
		//---------------------------------------------------------------------
		public int SaveTemplate(DMS_Collection collection, BookmarksDataTable fields)
		{
			try
			{
				string lockMsg = string.Empty;
				if (DMSOrchestrator.LockManager.LockRecord("DMS_CollectionsFields", collection.CollectionID.ToString(), DMSOrchestrator.LockContext, ref lockMsg))
				{
					//first I delete all collection fields 
					var collFields = from fc in dc.DMS_CollectionsFields
									 where fc.CollectionID == collection.CollectionID
									 select fc;

					string name = string.Empty;
					//inserisco quelli passati
					foreach (DataRow row in fields.Rows)
					{
						if (row.RowState == DataRowState.Deleted)
						{
							name = row[CommonStrings.Name, DataRowVersion.Original].ToString();
							if (collFields != null && collFields.Any(f => f.FieldName == name))
							{
								DMS_CollectionsField collField = collFields.Single(f => f.FieldName == name);
								collField.Disabled = true;
							}
							continue;
						}

						name = row[CommonStrings.Name].ToString();

						if (collFields != null && collFields.Any(f => f.FieldName == name))
						{
                            if (row.RowState == DataRowState.Modified || row.RowState == DataRowState.Added)//21190, anche added,il campo è già presente, magari però disabled...
							{
								DMS_CollectionsField collField = collFields.Single(f => f.FieldName == name);
								ModifyCollectionField(collField, row);
							}
						}
						else
							AddNewCollectionField(row, collection.CollectionID);
					}

					dc.SubmitChanges();
					DMSOrchestrator.LockManager.UnlockRecord("DMS_CollectionsFields", collection.CollectionID.ToString(), DMSOrchestrator.LockContext);
				}
				else
					SetMessage(string.Format(Strings.ErrorSavingTemplate, collection.TemplateName), new Exception(lockMsg), "SaveTemplate");		
			}
			catch (Exception e)
			{
				SetMessage(string.Format(Strings.ErrorSavingTemplate, collection.TemplateName), e, "SaveTemplate");
			}

			return collection.CollectionID;
		}

        //---------------------------------------------------------------------
        private long CreateDocumentCRC(DocumentToArchive docToArchive)
        {
            if (
                    (docToArchive.IsAFile && !File.Exists(docToArchive.DocumentPath)) ||
                    (!docToArchive.IsAFile && docToArchive.BinaryContent == null)
                )
                return 0;
            try
            {
                byte[] mHash = null;
                System.Security.Cryptography.MD5 sscMD5 = System.Security.Cryptography.MD5.Create();
                if (docToArchive.IsAFile)
                {
                    using (FileStream fs = File.OpenRead(docToArchive.DocumentPath))
                    {

                        // recuperiamo i bytes dell'hash
                        mHash = sscMD5.ComputeHash(fs);
                    }
                }
                else
                    mHash = sscMD5.ComputeHash(docToArchive.BinaryContent);
                // conversione a 64 bit

                string crc = Convert.ToBase64String(mHash);
                return BitConverter.ToInt64(mHash, 0);

            }
            catch (IOException ioe)
            {
                SetMessage(string.Format(Strings.ErrorFileInUse, docToArchive.DocumentPath), ioe, "CreateDocumentCRC");
            }

            catch (Exception e)
            {
                SetMessage(string.Format(Strings.ErrorCalculatingCRC, docToArchive.DocumentPath), e, "CreateDocumentCRC");
            }

            return 0;
        }


		//---------------------------------------------------------------------
		private long CreateDocumentCRC(AttachmentInfo attInfo)
		{
			if (attInfo.DocContent == null && !attInfo.VeryLargeFile)
				return 0;
			try
			{
				byte[] mHash = null;
				System.Security.Cryptography.MD5 sscMD5 = System.Security.Cryptography.MD5.Create();
				if (attInfo.VeryLargeFile)
				{
					using (FileStream fs = File.OpenRead(attInfo.TempPath))
					{
						// recuperiamo i bytes dell'hash
						mHash = sscMD5.ComputeHash(fs);
					}
				}
				else
					mHash = sscMD5.ComputeHash(attInfo.DocContent);

				// conversione a 64 bit
				string crc = Convert.ToBase64String(mHash);
				return BitConverter.ToInt64(mHash, 0);
			}
			catch (IOException ioe)
			{
				SetMessage(string.Format(Strings.ErrorFileInUse, attInfo.TempPath), ioe, "CreateDocumentCRC");
			}

			catch (Exception e)
			{
				SetMessage(string.Format(Strings.ErrorCalculatingCRC, attInfo.TempPath), e, "CreateDocumentCRC");
			}

			return 0;
		}


		///---------------------------------------------------------------------
		private bool UpdateBinaryContent(DMS_ArchivedDocument archivedDoc, DocumentToArchive docToArchive, bool newDocument)
        {
            bool success = false;

            DateTime start = DateTime.Now;
            Debug.WriteLine("Start UpdateBinaryContent");

            try
            {
                string lockMsg = string.Empty;

                if (DMSOrchestrator.LockManager.LockRecord(archivedDoc, DMSOrchestrator.LockContext, ref lockMsg))
                {
                    using (SqlConnection myConnection = new SqlConnection(DMSOrchestrator.DMSConnectionString))
                    {
                        myConnection.Open();
                        using (SqlCommand myCommand = myConnection.CreateCommand())
                        {
                            bool storageToFileSystem = DMSOrchestrator.SettingsManager.UsersSettingState.Options.StorageOptionsState.StorageToFileSystem;
                            if (newDocument)
                            {
                                if (storageToFileSystem)
                                {
                                    myCommand.CommandText = string.Format
                                    (
                                       @"INSERT INTO DMS_ArchivedDocContent (ArchivedDocID, BinaryContent, ExtensionType, OCRProcess, StorageFile) VALUES ({0}, 0x, 'path', 1, @storageFile)",
                                       archivedDoc.ArchivedDocID.ToString()
                                    );

                                    string folderName = DMSOrchestrator.SettingsManager.UsersSettingState.Options.StorageOptionsState.StorageFolderPath;
                                    string targetFileName = Path.Combine(folderName, Utils.ShrinkPath(folderName, Utils.GetTempFileName(archivedDoc)));
                                    if (docToArchive.IsAFile)
                                        File.Copy(docToArchive.DocumentPath, targetFileName, true);
									else
										File.WriteAllBytes(targetFileName, docToArchive.BinaryContent);

									FileInfo target = new FileInfo(targetFileName);
                                    target.IsReadOnly = false;
                                    myCommand.Parameters.Add(new SqlParameter("@storageFile", targetFileName));
                                }
                                else
                                {
                                    myCommand.CommandText = string.Format
                                   (
                                       @"INSERT INTO DMS_ArchivedDocContent (ArchivedDocID, BinaryContent, ExtensionType, OCRProcess, StorageFile) VALUES ({0}, @binaryCont, '{1}', @ocrProcess, '')",
                                       archivedDoc.ArchivedDocID.ToString(),
                                       archivedDoc.ExtensionType.ToString()
                                   );
                                    myCommand.Parameters.Add(new SqlParameter("@ocrProcess", (storageToFileSystem || DMSOrchestrator.FullTextFilterManager.IsIFilterDocType(archivedDoc.ExtensionType.ToString()))));
                                    myCommand.Parameters.Add(new SqlParameter("@binaryCont", (docToArchive.IsAFile) ? File.ReadAllBytes(docToArchive.DocumentPath) : docToArchive.BinaryContent));

                                }

                                myCommand.ExecuteNonQuery();

                            }
                            else
                            {

                                //se è di tipo file system allora nel campo ho il nome del file presente nel repository su file system                                  
                                if (archivedDoc.StorageType == (int)StorageTypeEnum.FileSystem)
                                {
                                    myCommand.CommandText = string.Format(@"SELECT StorageFile FROM DMS_ArchivedDocContent WHERE ArchivedDocID = {0}", archivedDoc.ArchivedDocID.ToString());
                                    string targetFileName = (string)myCommand.ExecuteScalar();
                                    bool isEmptyStorageFile = string.IsNullOrEmpty(targetFileName);
                                    if (isEmptyStorageFile)
                                    {
                                        string folderName = DMSOrchestrator.SettingsManager.UsersSettingState.Options.StorageOptionsState.StorageFolderPath;
                                        targetFileName = Path.Combine(folderName, Utils.ShrinkPath(folderName, Utils.GetTempFileName(archivedDoc)));
                                    }
                                    if (docToArchive.IsAFile)
                                        File.Copy(docToArchive.DocumentPath, targetFileName, true);
									else
										File.WriteAllBytes(targetFileName, docToArchive.BinaryContent);

									FileInfo target = new FileInfo(targetFileName);
                                    target.IsReadOnly = false;
                                    if (isEmptyStorageFile)
                                    {
                                        myCommand.CommandText = string.Format(@"UPDATE DMS_ArchivedDocContent SET StorageFile = @storageFile where ArchivedDocID = {0}", archivedDoc.ArchivedDocID.ToString());
                                        myCommand.Parameters.Add(new SqlParameter("@StorageFile", targetFileName));
                                        myCommand.ExecuteNonQuery();
                                    }
                                }
                                else
                                {
                                    myCommand.CommandText = string.Format(@"UPDATE DMS_ArchivedDocContent SET BinaryContent = @binaryCont, OCRProcess =  @ocrProcess where ArchivedDocID = {0}", archivedDoc.ArchivedDocID.ToString());
                                    myCommand.Parameters.Add(new SqlParameter("@binaryCont", (docToArchive.IsAFile) ? File.ReadAllBytes(docToArchive.DocumentPath) : docToArchive.BinaryContent));
                                    myCommand.Parameters.Add(new SqlParameter("@ocrProcess", DMSOrchestrator.FullTextFilterManager.IsIFilterDocType(archivedDoc.ExtensionType.ToString())));
                                    myCommand.ExecuteNonQuery();
                                }
                            }
                        }
                        success = true;
                    }
                }
                else
                    SetMessage(Strings.ErrorLoadingArchivedDoc, new Exception(lockMsg), "UpdateBinaryContent");
            }
            catch (UnauthorizedAccessException uae)
            {
                SetMessage(Strings.ErrorSavingBinaryContent, uae, "UpdateBinaryContent");
            }
            catch (SqlException e)
            {
                SetMessage(Strings.ErrorSavingBinaryContent, e, "UpdateBinaryContent");
            }
            catch (OutOfMemoryException oute)
            {
                // nel caso di eccezione OutOfMemoryException tento nuovamente di archiviare 
                // il documento inserendone un pezzo alla volta
                if (docToArchive.IsAFile)
                    success = UpdateBinaryContentForBigFile(archivedDoc, docToArchive.DocumentPath, newDocument);
                else
                {
                    SetMessage(string.Format(Strings.ErrorArchivingTooLargeFile, docToArchive.DocumentPath, "1.2"), oute, "UpdateBinaryContent");
                    success = false;
                }
            }
            catch (Exception e)
            {
                SetMessage(Strings.ErrorSavingBinaryContent, e, "UpdateBinaryContent");
            }
            finally
            {
                DMSOrchestrator.LockManager.UnlockRecord(archivedDoc, DMSOrchestrator.LockContext);
            }

            TimeSpan elapesed = DateTime.Now - start;
            Debug.WriteLine(string.Format("End UpdateBinaryContent in {0}", elapesed.ToString()));

            return success;
        }

		//---------------------------------------------------------------------
		private bool UpdateBinaryContentForBigFile(DMS_ArchivedDocument archivedDoc, string docPath, bool newDocument)
		{
			bool success = false;

			try
			{
				using (SqlConnection myConnection = new SqlConnection(DMSOrchestrator.DMSConnectionString))
				{
					myConnection.Open();

					using (SqlCommand myCommand = myConnection.CreateCommand())
					{
						// se il documento e' nuovo inserisco una riga con il Content vuoto
						myCommand.CommandText =  (newDocument)
									? string.Format(@"INSERT INTO DMS_ArchivedDocContent (ArchivedDocID, BinaryContent, ExtensionType, OCRProcess) VALUES ({0}, 0x0, '{1}', @ocrProcess)", archivedDoc.ArchivedDocID.ToString(), archivedDoc.ExtensionType.ToString())
									: string.Format(@"UPDATE DMS_ArchivedDocContent SET BinaryContent = 0x0, OCRProcess = @ocrProcess where ArchivedDocID = {0}", archivedDoc.ArchivedDocID.ToString());

						myCommand.Parameters.Add(new SqlParameter("@ocrProcess", DMSOrchestrator.FullTextFilterManager.IsIFilterDocType(archivedDoc.ExtensionType.ToString())));
						myCommand.ExecuteNonQuery();
						
						// in ogni caso devo aggiornare il BinaryContent scrivendo un pezzetto alla volta
						myCommand.CommandText = string.Format(@"UPDATE [DMS_ArchivedDocContent] SET [BinaryContent].WRITE(@content,  @offset, @len) WHERE ArchivedDocID = {0}", archivedDoc.ArchivedDocID.ToString());

						SqlParameter contentParam = new SqlParameter("@content", SqlDbType.VarBinary);
						myCommand.Parameters.Add(contentParam);

						SqlParameter offsetParam = new SqlParameter("@offset", SqlDbType.BigInt);
						myCommand.Parameters.Add(offsetParam);

						SqlParameter lengthParam = new SqlParameter("@len", SqlDbType.BigInt);
						myCommand.Parameters.Add(lengthParam);
						
						using (FileStream fs = new FileStream(docPath, FileMode.Open, FileAccess.Read, FileShare.Read))
						{
							// leggiamo a blocchi di 30MB
							int bufferSize = (archivedDoc.Size > 31457280) ? 31457280 : (int)archivedDoc.Size;
							
							byte[] buffer = new byte[bufferSize]; // chunk sizes 50MB
							int read = 0;
							int offset = 0;

							while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
							{
								contentParam.Value = buffer;
								offsetParam.Value = offset;
								lengthParam.Value = read;
								myCommand.ExecuteNonQuery();
								offset += read;
							}
							buffer = null;
						}

						myCommand.ExecuteNonQuery();
					}

					success = true;
				}
			}
			catch (SqlException e)
			{
				SetMessage(Strings.ErrorSavingBinaryContent, e, "UpdateBinaryContentForBigFile");
			}
			catch (OutOfMemoryException e)
			{
				SetMessage(Strings.ErrorSavingBinaryContent, e, "UpdateBinaryContentForBigFile");
			}

			return success;
		}

		//---------------------------------------------------------------------
		private void UpdateAttachmentsModifiedData(DMS_ArchivedDocument archivedDoc)
		{
			List<int> attachmentList = searchManager.GetAttachmentsForArchivedDocId(archivedDoc.ArchivedDocID);
	
			foreach(int attachmentId in attachmentList)
			{
                DMS_Attachment attachment = searchManager.GetAttachment(attachmentId);
				if (attachment == null)
					continue;

				try
				{
					string lockMsg = string.Empty;
					if (DMSOrchestrator.LockManager.LockRecord(attachment, DMSOrchestrator.LockContext, ref lockMsg))
					{
						attachment.TBModified = archivedDoc.TBModified;
						attachment.TBModifiedID = archivedDoc.TBModifiedID;
						dc.SubmitChanges();
						DMSOrchestrator.LockManager.UnlockRecord(attachment, DMSOrchestrator.LockContext);
					}
					else
                        SetMessage(Strings.ErrorUpdatingSearchIndexes, new Exception(lockMsg), "UpdateAttachmentsModifiedData");
				}
				catch (Exception e)
				{
                    SetMessage(Strings.ErrorUpdatingSearchIndexes, e, "UpdateAttachmentsModifiedData");
				}
			}
		}

        //---------------------------------------------------------------------
        private bool ReplaceExistingDocument(ref DMS_ArchivedDocument oldArchiveDoc, DocumentToArchive docToArchive, bool fromCheckIn = false)
        {
            if (
					DMSOrchestrator.SettingsManager.UsersSettingState.Options.RepositoryOptionsState.ShowOnlyMyArchivedDocs &&
					oldArchiveDoc.TBModifiedID != DMSOrchestrator.WorkerId
				)
			{
				SetMessage(Strings.ErrorLoadingArchivedDoc, null, Strings.DocAlreadyArchived);
				return false;
			}

            FileInfo file = new FileInfo(docToArchive.DocumentPath);
            // todo controllare aggiornamento barcode in caso di valore diverso in un file nella stessa posizione
            string lockMsg = string.Empty;
            DateTime dbDate;
            if (UpdateBinaryContent(oldArchiveDoc, docToArchive, false))
            {
                dbDate = dc.GetDate();
                oldArchiveDoc.LastWriteTimeUtc = file.LastWriteTimeUtc;
                oldArchiveDoc.TBModified = dbDate;
                oldArchiveDoc.Size = file.Length;
                oldArchiveDoc.TBModifiedID = DMSOrchestrator.WorkerId;
                oldArchiveDoc.CRC = docToArchive.CRC; ;
                if (!fromCheckIn)
                {
                    //se la descrizione era uguale al nome file la cambio ( vuol dire che era rimasta al default), se no lascio uguale...
                    if (String.Compare(oldArchiveDoc.Name, oldArchiveDoc.Description, true) == 0)
                        oldArchiveDoc.Description = file.Name;
                    oldArchiveDoc.Name = file.Name;
                    oldArchiveDoc.Path = file.DirectoryName;
                    oldArchiveDoc.ExtensionType = file.Extension;
                }
				dc.SubmitChanges();

				//devo modificare anche la data di modifica di tutti gli allegati che fanno riferimento a questo documento archiviato
				UpdateAttachmentsModifiedData(oldArchiveDoc);
				return true;
			}

			return false;
		}
						
		//---------------------------------------------------------------------
        protected bool GetArchivedDocumentInfo(out AttachmentInfo attachmentInfo, ref DocumentToArchive docToArchive)
		{
            attachmentInfo = null;
            if (
               (docToArchive.IsAFile && !File.Exists(docToArchive.DocumentPath)) ||
               (!docToArchive.IsAFile && docToArchive.BinaryContent == null)
               )
            {
                SetMessage(string.Format(Strings.FileNotExists, docToArchive.DocumentPath), null, "GetArchivedDocumentInfo");
				return false;
			}

			DateTime start = DateTime.Now;
			Debug.WriteLine("Start GetArchivedDocumentInfo");

			DMS_ArchivedDocument archiveDoc = null;
            DMS_ArchivedDocument newArchivedDoc = new DMS_ArchivedDocument();

            FileInfo file = new FileInfo(docToArchive.DocumentPath);
			if (file == null) return false;

			// check lunghezza estensione
			if (file.Extension.Length > 10)
			{
				SetMessage(string.Format(Strings.ExtensionTooLong, file.FullName), null, "GetArchivedDocumentInfo");
                return false;
			}

			// se nei parametri sono state specificate le estensioni da escludere per gli allegati vado a controllare
			if (!string.IsNullOrWhiteSpace(DMSOrchestrator.SettingsManager.UsersSettingState.Options.RepositoryOptionsState.ExcludedExtensions))
			{
				string[] extensionsArray = DMSOrchestrator.SettingsManager.UsersSettingState.Options.RepositoryOptionsState.ExcludedExtensions.Split(';');

				bool bToExclude = false;
				string ext = string.Empty;
				for (int i = 0; i < extensionsArray.Length; i++)
				{
					ext = extensionsArray[i].Trim();
					if (!ext.StartsWith(".")) ext = "." + ext;

					if (string.Compare(file.Extension, ext.Trim(), StringComparison.InvariantCultureIgnoreCase) == 0)
					{
						bToExclude = true;
						break;
					}
				}

				if (bToExclude)
				{
					SetMessage(string.Format(Strings.NoAdmittedExtension, file.FullName), null, "GetArchivedDocumentInfo");
					return false;
				}
			}

			// check dimensione massima per tipo estensione
			ExtensionSize es = DMSOrchestrator.SettingsManager.UsersSettingState.Options.RepositoryOptionsState.GetExtensionSize(file.Extension);
			if (es != null)
			{
				long maxSizeForExtension = es.SizeInBytes;
				if (maxSizeForExtension > 0 && file.Length > maxSizeForExtension)
				{
					SetMessage(string.Format(Strings.FileSizeTooBig, file.FullName, es.Size.ToString()), null, "GetArchivedDocumentInfo");
					return false;
				}
			}

			docToArchive.CRC = CreateDocumentCRC(docToArchive);

			newArchivedDoc.ExtensionType = file.Extension;
            newArchivedDoc.Path = file.DirectoryName;
            newArchivedDoc.Name = file.Name;
            newArchivedDoc.Language = this.DMSOrchestrator.OCRDictionary.CultureName;
            newArchivedDoc.Description = docToArchive.Description;
            newArchivedDoc.Description = (!string.IsNullOrEmpty(docToArchive.Description)) ? docToArchive.Description : newArchivedDoc.Name;
            newArchivedDoc.CreationTimeUtc = docToArchive.CreationTimeUtc;
            newArchivedDoc.LastWriteTimeUtc = docToArchive.LastWriteTimeUtc;
            newArchivedDoc.Size = docToArchive.Size;
            newArchivedDoc.TBCreatedID = DMSOrchestrator.WorkerId;
            newArchivedDoc.TBModifiedID = DMSOrchestrator.WorkerId;
            newArchivedDoc.CollectionID = DMSOrchestrator.RepositoryCollection.CollectionID;
            newArchivedDoc.IsWoormReport = docToArchive.IsWoormReport;
            newArchivedDoc.StorageType = (DMSOrchestrator.SettingsManager.UsersSettingState.Options.StorageOptionsState.StorageToFileSystem) ? (int)StorageTypeEnum.FileSystem : (int)StorageTypeEnum.Database;
            newArchivedDoc.CRC = docToArchive.CRC;

            bool newDoc = false;
			string docBarcode = string.Empty;

            if (DMSOrchestrator.SettingsManager.UsersSettingState.Options.BarcodeDetectionOptionsState.EnableBarcode)
            {
                if (docToArchive.SkipDetectBarcode)
                {
                    newArchivedDoc.Barcode = docBarcode = docToArchive.Barcode;
                    newArchivedDoc.BarcodeType = DMSOrchestrator.SettingsManager.UsersSettingState.Options.BarcodeDetectionOptionsState.BarcodeTypeValue;
                }
                else
                {
                    if (DMSOrchestrator.SettingsManager.UsersSettingState.Options.BarcodeDetectionOptionsState.AutomaticBarcodeDetection)
                    {
                        AttachmentInfo att = new AttachmentInfo(docToArchive, DMSOrchestrator);
                        TypedBarcode tb = DetectBarcode(att);
                        newArchivedDoc.Barcode = docBarcode = tb.Value;
                        newArchivedDoc.BarcodeType = BarcodeMapping.GetBarCodeDescription(tb.Type);
                        att.Dispose();
                    }
                    else
                    {
                        newArchivedDoc.Barcode = docBarcode = string.Empty;
                        newArchivedDoc.BarcodeType = BarcodeMapping.GetBarCodeDescription(BarCodeType.NONE);
                    }
                }


                if (!String.IsNullOrWhiteSpace(newArchivedDoc.Barcode))
                    newArchivedDoc.BarcodeType = DMSOrchestrator.SettingsManager.UsersSettingState.Options.BarcodeDetectionOptionsState.BarcodeTypeValue;
                else newArchivedDoc.BarcodeType = "";
            }

			//if is a woorm report I search for an archived document with same name, extension only 
			//else i consider the creationtime also
			var var =  (!string.IsNullOrEmpty(docBarcode))
						?
							(from doc in dc.DMS_ArchivedDocuments
							where doc.Barcode == docBarcode
							orderby doc.TBModified descending
							select doc)
						:
						(
							(docToArchive.IsWoormReport) //bugfix #19225
							? 
								(from doc in dc.DMS_ArchivedDocuments
							   //where doc.Name == file.Name && doc.ExtensionType == file.Extension
								 where doc.Name == newArchivedDoc.Name && doc.ExtensionType == newArchivedDoc.ExtensionType
								 orderby doc.TBModified descending
							   select doc)
						   :
							   (from doc in dc.DMS_ArchivedDocuments
							   where doc.Name == newArchivedDoc.Name && doc.ExtensionType == newArchivedDoc.ExtensionType
							   && doc.CreationTimeUtc == newArchivedDoc.CreationTimeUtc
							   orderby doc.TBModified descending
							   select doc) 
						  );
    
			bool insertNew = false;
			try
			{
                insertNew = (var == null || !var.Any());

		        if (!insertNew)
		        {
			        DMS_ArchivedDocument oldArchiveDoc = var.First();
					//BugFix #23216
					//se il CRC è a 0 lo devo ricalcolare
					if (oldArchiveDoc.CRC == 0)
					{
						AttachmentInfo oldAttInfo = new AttachmentInfo(oldArchiveDoc, DMSOrchestrator);
						oldArchiveDoc.CRC = CreateDocumentCRC(oldAttInfo);
						dc.SubmitChanges();

						oldAttInfo.Dispose();
					}

					existingAction = duplicateManager.CheckDuplicateArchivedDocument(oldArchiveDoc, newArchivedDoc);
					
			        switch (existingAction)
			        {
				        case DuplicateDocumentAction.ReplaceExistingDoc:
					        if (ReplaceExistingDocument(ref oldArchiveDoc, docToArchive))
						        archiveDoc = oldArchiveDoc;
					        else
						        return false;
					        break;

				        case DuplicateDocumentAction.UseExistingDoc:
					        {
						        if (
							        DMSOrchestrator.SettingsManager.UsersSettingState.Options.RepositoryOptionsState.ShowOnlyMyArchivedDocs &&
							        oldArchiveDoc.TBModifiedID != DMSOrchestrator.WorkerId
						        )
						        {
							        SetMessage(Strings.NoGrantToModifyArchivedDoc, null, "", DiagnosticType.Information);
							        return false;
						        }

						        archiveDoc = oldArchiveDoc;
						        break;
					        }

				        case DuplicateDocumentAction.RefuseAttachmentOperation:
					        SetMessage(string.Format(Strings.MsgRefuseAttachDoc, Strings.EnumDuplicateDocOption5), null, "GetArchivedDocumentInfo", DiagnosticType.Information);
                            return true;

				        case DuplicateDocumentAction.ArchiveAndKeepBothDocs:
					        insertNew = true;
					        break;

				        case DuplicateDocumentAction.Cancel:
                            return true;
			        }
		        }

		        if (insertNew) // se il documento non esiste nel repository oppure se l'utente ha deciso di inserirne il duplicato( ArchiveAndKeepBothDocs )
		        {
			        // it's a new archived document
			        dc.DMS_ArchivedDocuments.InsertOnSubmit(newArchivedDoc);
			        dc.SubmitChanges();
			        newArchivedDoc.TBModified = dc.GetDate();			
			        dc.SubmitChanges();

			        archiveDoc = newArchivedDoc;
                    newDoc = true;
                    if (!UpdateBinaryContent(newArchivedDoc, docToArchive, true))
                    {
                        dc.DMS_ArchivedDocuments.DeleteOnSubmit(newArchivedDoc);
				        dc.SubmitChanges();
                        return false;
			        }
		        }

		        attachmentInfo = new AttachmentInfo(archiveDoc, DMSOrchestrator);

		        if (newDoc)
		        {
                    attachmentInfo.BookmarksDT = GetCollectionFields(DMSOrchestrator.RepositoryCollection.CollectionID, BookmarksDataTable.DisableFilter.OnlyEnable);
			        InsertIndexes(attachmentInfo, string.Empty);
		        }
            }
            catch (OutOfMemoryException outExc) //this one fails with outofmemory exception for very large file (like avi)
			{
             	SetMessage(Strings.ErrorLoadingArchivedDoc, outExc, "GetArchivedDocumentInfo");
				attachmentInfo = null;
                return false;
			}
			catch (SqlException sqlExc)
			{
				SetMessage(Strings.ErrorLoadingArchivedDoc, sqlExc, "GetArchivedDocumentInfo");
				attachmentInfo = null;
                return false;
			}
			catch (Exception e)
			{
				SetMessage(Strings.ErrorLoadingArchivedDoc, e, "GetArchivedDocumentInfo");
				attachmentInfo = null;
                return false;
			}
			
			TimeSpan elapesed = DateTime.Now - start;
			Debug.WriteLine(string.Format("End GetArchivedDocumentInfo in {0}", elapesed.ToString()));
			
			return true;
		}

		///<summary>
		/// Dato un file si esegue il detect dei barcode eventualmente presenti e si assegna all'allegato
		/// corrente il suo valore
		///</summary>
		//---------------------------------------------------------------------
		public TypedBarcode DetectBarcode(AttachmentInfo ai)
		{
			DateTime start = DateTime.Now;
			Debug.WriteLine("Start DetectBarcode");

            TypedBarcode barcode = DMSOrchestrator.BarcodeManager.DetectBarcodeValueInFile(ai);
			TimeSpan elapsed = DateTime.Now - start;
			Debug.WriteLine(string.Format("End DetectBarcode in {0}", elapsed.ToString()));

			return barcode;
		}


        //Archivia e ritorna archiveddocid appena inserito. Effettua anche la riconciliazione con un eventuale papery. Se bForceAttachPapery = true non effettua alcuna richiesta all'utente
        //---------------------------------------------------------------------
        public ArchiveResult ArchiveDocument(ref DocumentToArchive docToArchive, out int archiveddocid)
        {
            archiveddocid = -1;

            // click with no selected document
            FileInfo f = new FileInfo(docToArchive.DocumentPath);
            if (
                    (docToArchive.IsAFile && f.Length == 0) ||
                    (!docToArchive.IsAFile && (docToArchive.BinaryContent == null || docToArchive.BinaryContent.Count() == 0))
                )
            {
                SetMessage(String.Format(Strings.FileIsEmpty, docToArchive.DocumentPath), null, "ArchiveDocument");
                return ArchiveResult.TerminatedWithError;
            }

            try
            {
                //first check if the document is already in repository
                //first check if the document is already in repository
                AttachmentInfo archDocInfo = null;
                bool result = GetArchivedDocumentInfo(out archDocInfo, ref docToArchive);
                if (archDocInfo != null)
                {
                    archiveddocid = archDocInfo.ArchivedDocId;

                    if (defaultBookmarksDT == null)
                        defaultBookmarksDT = new BookmarksDataTable();
                        
                    //defaultBookmarksDT = GetRepositoryCollectionFields(DMSOrchestrator.RepositoryCollection);

					  // se l'archiviazione e' completata con successo vado ad effettuare l'eventuale riconciliazione dei barcode dei cartacei
					if (archiveddocid > 0)
                        DMSOrchestrator.UpdateBarcode(archDocInfo, archDocInfo.TBarcode.Value);
      
                    if (ArchiveCompleted != null)
                    {
                        AttachmentInfoEventArgs arg = new AttachmentInfoEventArgs();
                        arg.CurrentAttachment = archDocInfo;
                        ArchiveCompleted(this, arg);
                    }
                    return (existingAction == DuplicateDocumentAction.UseExistingDoc) ? ArchiveResult.Cancel : ArchiveResult.TerminatedSuccess;
                }
                else
                    return (result && existingAction == DuplicateDocumentAction.Cancel) ? ArchiveResult.Cancel : ArchiveResult.TerminatedWithError;
            }
            catch (OutOfMemoryException oute)
            {
                SetMessage(string.Format(Strings.ErrorArchivingTooLargeFile, docToArchive.DocumentPath, "1.2"), oute, "ArchiveDocument");
            }
            catch (SqlException sqle)
            {
                SetMessage(string.Format(Strings.ErrorArchivingFile, docToArchive.DocumentPath), sqle, "ArchiveDocument");
            }  
            catch (Exception e)
            {
                SetMessage(string.Format(Strings.ErrorArchivingFile, docToArchive.DocumentPath), e, "ArchiveDocument");
            }
           
            return ArchiveResult.TerminatedWithError;			  
        }

        //Archivia e ritorna archiveddocid appena inserito. 
        //---------------------------------------------------------------------
        public ArchiveResult ArchiveFile(string docPath, string description, out int archivedDocId, bool isWoormReport = false, bool skipDetectBarcode = false, string barcode = "")
        {
            archivedDocId = -1;

            // click with no selected document
            if (string.IsNullOrWhiteSpace(docPath))
            {
                SetMessage(Strings.ErrorPathIsEmpty, null, "ArchiveDocument");
                return ArchiveResult.TerminatedWithError;
            }

            DocumentToArchive docToArchive = new DocumentToArchive();
            docToArchive.IsAFile = true;
            docToArchive.DocumentPath = docPath;
            docToArchive.Description = description;
            docToArchive.IsWoormReport = isWoormReport;
            docToArchive.Barcode = barcode;
            docToArchive.SkipDetectBarcode = skipDetectBarcode;

            return ArchiveDocument(ref docToArchive, out archivedDocId);
        }

        /// <summary>
        /// used to archive the document in the repository
        /// </summary>
        //---------------------------------------------------------------------
        public ArchiveResult ArchiveFile(string docPath, string description, bool isWoormReport)
        {
            int archivedDocId = -1;
            return ArchiveFile(docPath, description, out archivedDocId, isWoormReport);
        }

        //archivia un binario è comunque obbligatorio passare un sourceFileName (anche inesistente)
        //---------------------------------------------------------------------
        public ArchiveResult ArchiveBinaryContent(byte[] binaryContent, string sourceFileName, string description, out int archivedDocId, bool isWoormReport = false, bool skipDetectBarcode = false, string barcode = "")
        {
            archivedDocId = -1;
            // click with no selected document
            if (string.IsNullOrWhiteSpace(sourceFileName))
            {
                SetMessage(Strings.ErrorPathIsEmpty, null, "ArchiveBinaryContent");
                return ArchiveResult.TerminatedWithError;
            }

            DocumentToArchive docToArchive = new DocumentToArchive();
            docToArchive.IsAFile = false;
            docToArchive.BinaryContent = binaryContent;
            docToArchive.DocumentPath = sourceFileName;
            docToArchive.Description = description;
            docToArchive.IsWoormReport = isWoormReport;
            docToArchive.Barcode = barcode;
            docToArchive.SkipDetectBarcode = skipDetectBarcode;

            ArchiveResult archResult = ArchiveDocument(ref docToArchive, out archivedDocId);
            docToArchive.BinaryContent = null;
            return archResult;
        }

        //---------------------------------------------------------------------
        public ArchiveResult SubstituteDocumentUsingBarcode(string docPath, string description, string barcode , out int archivedDocId)
		{
            archivedDocId = -1;
			
			if (string.IsNullOrEmpty(barcode))
				return ArchiveResult.Cancel;
			
			DMS_ArchivedDocument oldDocument = barcodeManager.GetArchivedDocFromBarcodeValue(barcode);

			if (oldDocument == null)
			{
				SetMessage((String.Format(Strings.ErrorArchivingFile, docPath) + "The document to substitute doesn't exist"), null, "SubstituteDocumentUsingBarcode");
				return ArchiveResult.TerminatedWithError;
			}
            DocumentToArchive docToArchive = new DocumentToArchive();
            docToArchive.IsAFile = true;
            docToArchive.DocumentPath = docPath;
            docToArchive.Description = description;
            docToArchive.Barcode = barcode;

            long crc = CreateDocumentCRC(docToArchive);

            if (!ReplaceExistingDocument(ref oldDocument, docToArchive))
            {
                SetMessage(String.Format(Strings.ErrorArchivingFile, docPath), null, "SubstituteDocumentUsingBarcode");
				return ArchiveResult.TerminatedWithError;
			}

            archivedDocId = oldDocument.ArchivedDocID;

            return ArchiveResult.TerminatedSuccess;
		}


        //---------------------------------------------------------------------
        private bool DeleteBinaryContent(DMS_ArchivedDocument archivedDoc)
        {
            bool success = false;
            SqlConnection connection = null;
            SqlCommand command = null;

            try
            {

                connection = new SqlConnection(DMSOrchestrator.DMSConnectionString);
                connection.Open();
                command = new SqlCommand();
                command.Connection = connection;

                //se il binario è salvato su FileSystem
                //se è di tipo file system allora nel campo ho il nome del file presente nel repository su file system                
                if (archivedDoc.StorageType == (int)StorageTypeEnum.FileSystem)
                {
                    command.CommandText = string.Format(@"SELECT StorageFile FROM DMS_ArchivedDocContent WHERE ArchivedDocID = {0}", archivedDoc.ArchivedDocID.ToString());
                    string fileStoragePath = (string)command.ExecuteScalar();
                    if (File.Exists(fileStoragePath))
                        File.Delete(fileStoragePath);
                }

                command.CommandText = string.Format(@"DELETE DMS_ArchivedDocTextContent WHERE ArchivedDocID = {0}", archivedDoc.ArchivedDocID.ToString());
                command.ExecuteNonQuery();

                command.CommandText = string.Format(@"DELETE DMS_ArchivedDocContent WHERE ArchivedDocID = {0}", archivedDoc.ArchivedDocID.ToString());
                command.ExecuteNonQuery();

                success = true;
            }
            catch (SqlException e)
            {
                SetMessage(Strings.ErrorDeletingBinaryContent, e, "DeleteBinaryContent");
            }
            catch (OutOfMemoryException e)
            {
                SetMessage(Strings.ErrorDeletingBinaryContent, e, "DeleteBinaryContent");
            }
            catch (Exception e)
            {
                SetMessage(Strings.ErrorDeletingBinaryContent, e, "DeleteBinaryContent");
            }
            finally
            {
                if (command != null)
                    command.Dispose();

                if (connection != null && connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
            return success;
        }

	
        ///<summary>
		/// Elimino l'ArchiveDoc selezionato
		///</summary>
		//---------------------------------------------------------------------
		public bool DeleteArchiveDoc(int archivedDocId)
		{
			DMS_ArchivedDocument archivedDoc = searchManager.GetArchivedDocument(archivedDocId);
			if (archivedDoc == null)
				return false;
			
			try
			{
				string lockMsg = string.Empty;
				if (DMSOrchestrator.LockManager.LockRecord(archivedDoc, DMSOrchestrator.LockContext, ref lockMsg))
				{
					//per prima cosa cancello il contenuto binario
                    if (DeleteBinaryContent(archivedDoc))
                    {
						//per prima cosa cancello gli indici di ricerca
						searchManager.DeleteArchivedDocIndexes(archivedDocId);
						//ed in ultimo cancello la riga in DMS_ArchivedDocument
						dc.DMS_ArchivedDocuments.DeleteOnSubmit(archivedDoc);
						dc.SubmitChanges();
					}
					DMSOrchestrator.LockManager.UnlockRecord(archivedDoc, DMSOrchestrator.LockContext);					
				}
				else
					SetMessage(Strings.ErrorDeletingAttachment, new Exception(lockMsg), "DeleteArchiveDoc");
			}
			catch (Exception e)
			{
                SetMessage(Strings.ErrorDeletingAttachment, e, "DeleteArchiveDoc");
				return false;
			}

			return true;
		}

		///<summary>
		/// Elimino l'ArchiveDoc selezionato più tutti i suoi attachment
		///</summary>
		//---------------------------------------------------------------------
		public bool DeleteArchiveDocInCascade(AttachmentInfo attachmentInfo)
		{
			bool result = true;

			try
			{
				DMS_ArchivedDocument archivedDoc = searchManager.GetArchivedDocument(attachmentInfo.ArchivedDocId);
				if (archivedDoc == null)
					return result; //doc non esiste = cancellazione true

				string lockMsg = string.Empty;
				if (DMSOrchestrator.LockManager.LockRecord(archivedDoc, DMSOrchestrator.LockContext, ref lockMsg))
				{
					List<int> attachmentList = searchManager.GetAttachmentsForArchivedDocId(attachmentInfo.ArchivedDocId);

					foreach (int attachmentId in attachmentList)
                        result = DeleteDMS_Attachment(attachmentId) && result;

					if (result)
					{
						//per prima cosa cancello il contenuto binario
						if (DeleteBinaryContent(archivedDoc))
						{
							//poi gli indici di ricerca
							searchManager.DeleteIndexes(attachmentInfo);
							//ed in ultimo la riga in DMS_ArchivedDocument
							dc.DMS_ArchivedDocuments.DeleteOnSubmit(archivedDoc);
							dc.SubmitChanges();
						}
					}
					DMSOrchestrator.LockManager.UnlockRecord(archivedDoc, DMSOrchestrator.LockContext);
				}
				else
                    SetMessage(Strings.ErrorDeletingAttachment, new Exception(lockMsg), "DeleteArchiveDocInCascade");
				
				//if (AttachmentDeleted != null)
				//	AttachmentDeleted(this, new EventArgs());
			}
			catch (Exception e)
			{
                SetMessage(Strings.ErrorDeletingAttachment, e, "DeleteArchiveDocInCascade");
				return false;
			}

			return result;
		}

		///<summary>
		/// Dato un ArchiveDocId cerco gli eventuali attachment agganciati (escluso il corrente)
		/// se non ce ne fossero potrei anche eliminare il documento archiviato dal database
		///</summary>
		//---------------------------------------------------------------------
		public bool CanDeleteArchivedDocument(int archivedDocId, int attachmentId)
		{
			var attachment = (from att in dc.DMS_Attachments
							  where att.ArchivedDocID == archivedDocId
							  && att.AttachmentID != attachmentId
							  select att);

			return !(attachment != null && attachment.Any());
		}

		/// <summary>
		/// salva i valori degli indici di ricerca associati al documento archiviato
		/// </summary>
		//---------------------------------------------------------------------
		protected void InsertIndexes(AttachmentInfo attachmentInfo, string tags)
		{
			//I create the indexes
			try
			{
                //ottimizzazione                
				searchManager.InsertIndexesSql(attachmentInfo, tags, attachmentInfo.Description);
			}
			catch (Exception e)
			{
				throw (e);
			}
		}

		
        //---------------------------------------------------------------------
        private bool SetModifierId(AttachmentInfo attachmentInfo , int workerId, bool undo = false)
        {
			var var = (from att in dc.DMS_ArchivedDocuments
                       where att.ArchivedDocID == attachmentInfo.ArchivedDocId
					   select att);

			DMS_ArchivedDocument archived = null;
            try
            {
                archived = (var != null && var.Any()) ? (DMS_ArchivedDocument)var.Single() : null;

                if (archived == null)
                {
                    SetMessage(String.Format(Strings.ErrorCheckOutNoDoc, attachmentInfo.ArchivedDocId.ToString()), null, "SetModifierId");
                    return false;
                }

                //se uguali vuol dire che già lui stesso lo ha in out o che è già stato checkinato                
                //oppure sto facendo undo
                if (workerId == archived.ModifierID && !undo)
                    return true;

                if (workerId != archived.ModifierID && undo)//undo può farlo solo lo stesso utente
                {
                    SetMessage(Strings.ErrorCheckOutOtherUser + String.Format(Strings.FileName, archived.Name), null, "SetModifierId");
                    return false;
                }

                if (workerId != -1 && archived.ModifierID != null && archived.ModifierID != -1 && !undo)//vuol dire che già altri lo hanno in out
                {
                    SetMessage(Strings.ErrorCheckOutOtherUser + String.Format(Strings.FileName, archived.Name), null, "SetModifierId");
                    return false;
                }

                string lockMsg = string.Empty;
                if (DMSOrchestrator.LockManager.LockRecord(archived, DMSOrchestrator.LockContext, ref lockMsg))
                {
                    if (workerId == archived.ModifierID && undo)
                        archived.ModifierID = null;
              
                    else
                        archived.ModifierID = workerId;
                    dc.SubmitChanges();

                    attachmentInfo.ModifierID = (archived.ModifierID == null) ? -1 : (int)archived.ModifierID;               
                    bool unlock = DMSOrchestrator.LockManager.UnlockRecord(archived, DMSOrchestrator.LockContext);
                    
                    if (!unlock) SetMessage(Strings.ErrorCheckOutIn, null, "SetModifierId");
                    return unlock;
                }
            }
            catch (Exception e)
            {
                SetMessage(string.Format(Strings.ErrorUpdatingArchivedDoc, attachmentInfo.ArchivedDocId.ToString()), e, "SetModifierId");

            }
            SetMessage(Strings.ErrorCheckOutIn, null, "SetModifierId");
            return false;
        }

        //---------------------------------------------------------------------
        private bool VerifySOSStatus(int archivedDocId)
        {
            List<int> attachs = searchManager.GetAttachmentsForArchivedDocId(archivedDocId);

            foreach (int item in attachs)
            {
                SOSDocumentInfo sosDocInfo = sosManager.GetSosDocumentInfo(item);

                if (
                    sosDocInfo != null
                    && (StatoDocumento)sosDocInfo.DocumentStatus != StatoDocumento.IDLE
                    && (StatoDocumento)sosDocInfo.DocumentStatus != StatoDocumento.DOCKO
                    && (StatoDocumento)sosDocInfo.DocumentStatus != StatoDocumento.TORESEND
                    )
                {
                    SetMessage(Strings.ErrorSOSDocNotIdle + String.Format(Strings.FileName, sosDocInfo.FileName), null, "VerifySOSStatus");
                    return false;
                }
            }
            return true;
        }
        //---------------------------------------------------------------------
        internal bool CheckOut(AttachmentInfo ai, int workerId)
        {
           if (!VerifySOSStatus(ai.ArchivedDocId)) return false;

           //imposto il readonly = false
           FileInfo f = new FileInfo(ai.TempPath);
            if (!f.Exists && !DMSOrchestrator.SaveAttachmentFile(ai))
            {
                SetMessage(Strings.ErrorCheckInFileDoesntExist + String.Format(Strings.FileName, ai.TempPath), null, "CheckOut");
                return false;
           }

           if (f.IsReadOnly)
               f.Attributes = f.Attributes & ~FileAttributes.ReadOnly;

           return SetModifierId(ai, workerId);
        }

        //---------------------------------------------------------------------
        internal bool Undo(AttachmentInfo ai, int workerId)
        {
            try
            {
                FileInfo fi = new FileInfo(ai.TempPath);
                if (fi.Exists) 
                { 
                    fi.IsReadOnly = false; 
                    fi.Delete(); 
                }  
            }
            catch (Exception e)
            {
                SetMessage(String.Format(Strings.ErrorUndoFile, ai.TempPath), e, "Undo");
				return false;
            }

            return SetModifierId(ai, workerId, true);
        }

		/// <summary>
		/// effettua il check in 
		/// </summary>
		//---------------------------------------------------------------------
		internal bool CheckIn(AttachmentInfo ai)
		{
			//reimposto il readonly
			FileInfo f = new FileInfo(ai.TempPath);
			if (!f.Exists)
			{
				SetMessage(Strings.ErrorCheckInFileDoesntExist + String.Format(Strings.FileName, ai.TempPath), null, "CheckIn");
				return false;
			}

			DMS_ArchivedDocument olddoc = searchManager.GetArchivedDocument(ai.ArchivedDocId);
            DocumentToArchive docToArchive = new DocumentToArchive();
            docToArchive.IsAFile = true;
            docToArchive.DocumentPath = ai.TempPath;
            docToArchive.CRC = CreateDocumentCRC(docToArchive);
            if (docToArchive.CRC == 0)
                return false;
            bool res = ReplaceExistingDocument(ref olddoc, docToArchive, true);

            f.IsReadOnly = true;
            f.Attributes = f.Attributes & ~FileAttributes.Hidden;
            f.Attributes = f.Attributes & ~FileAttributes.System;
            f.Refresh();

            if (!res)
            {
                SetMessage(Strings.ErrorCheckInFileReplacing + String.Format(Strings.FileName, ai.Name), null, "CheckIn");
                return false;
            }

            UpdateSize(ai, f);
            ai.SetModifiedBy();
            return SetModifierId(ai, -1);
        }

        //---------------------------------------------------------------------
        private void UpdateSize(AttachmentInfo ai, FileInfo f)
        {
            long OriginalSize = f.Length;
            ai.KBSize = (OriginalSize > 1024) ? OriginalSize / 1024 : 1;
        }


        //----------------------------------------------------------------------
        public void ResetDuplicatedErpDocId()
        {
            //trovo se ci son duplicati
            string query = "SELECT PrimaryKeyValue, DocNamespace from DMS_ErpDocument group by PrimaryKeyValue, DocNamespace HAVING count(*) > 1 ";
            string docNamespace;
            string primaryKeyValue;

            SqlCommand aSqlCommand = new SqlCommand();
            SqlDataReader reader = null;
            try
            {
                using (SqlConnection myConnection = new SqlConnection(DMSOrchestrator.DMSConnectionString))
                {
                    myConnection.Open();
                    aSqlCommand.CommandText = query;
                    aSqlCommand.Connection = myConnection;

                    reader = aSqlCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        primaryKeyValue = (string)reader["PrimaryKeyValue"];
                        docNamespace = (string)reader["DocNamespace"];
                        //per ogni coppia trovata vado a fare le query necessarie a trovare i duplicati.
                        FindDuplicatedInTables(primaryKeyValue, docNamespace/*, myConnection*/);
                    }
                }
            }
            catch (Exception exc)
            {
                SetMessage(string.Format(Strings.Error, exc.Message), exc, "FindDuplicatedErpDocID");

            }
            finally
            {
                if (aSqlCommand != null)
                    aSqlCommand.Dispose();

                if (reader != null && !reader.IsClosed)
                    reader.Close();
            }
        }

        //----------------------------------------------------------------------
        private void FindDuplicatedInTables(string primaryKeyValue, string docNamespace/*, SqlConnection myConnection*/)
        {
            //per ogni namespace
            int PRIMOID = 0;
            string values = "({0})";
            string inWhereClause = string.Empty;
            string vals = string.Empty;

            //select ErpDocumentID from DMS_ErpDocument where DocNamespace = 'Document.ERP.SaleOrders.Documents.SaleOrd' and PrimaryKeyValue = 'SaleOrdId:10027;'
            SqlCommand aSqlCommand = new SqlCommand();
            using (SqlConnection myConnection = new SqlConnection(DMSOrchestrator.DMSConnectionString))
            {
                myConnection.Open();
                aSqlCommand.Connection = myConnection;
                aSqlCommand.CommandText = @"SELECT ErpDocumentID FROM DMS_ErpDocument where DocNamespace = @namespace and PrimaryKeyValue = @pk";
                aSqlCommand.Parameters.AddWithValue("@namespace", docNamespace);
                aSqlCommand.Parameters.AddWithValue("@pk", primaryKeyValue);
                List<int> ids = new List<int>();
                using (SqlDataReader aReader = aSqlCommand.ExecuteReader())
                {
                    if (!aReader.HasRows) { aReader.Close(); return; }
                    while (aReader.Read())
                    {
                        int erpDocumentID = (int)aReader["ErpDocumentID"];
                        ids.Add(erpDocumentID);
                    }
                    ids.Sort();
                    if (ids.Count <= 1)//nessun duplicato!
                        return;
                    PRIMOID = ids[0];
                    ids.RemoveAt(0);//tolgo il più basso è quello che prendo come valore da sostituire, quindi rimane
                    for (int i = 0; i < ids.Count; i++)
                    {
                        if (i > 0) vals += ",";
                        vals += ids[i].ToString();
                    }
                    inWhereClause = string.Format(values, vals);

                }


                //select * from DMS_ErpDocBarcodes         where ErpDocumentID in 
                //select * from DMS_IndexesSynchronization where ErpDocumentID in
                //select * from DMS_Attachment             where ErpDocumentID in

                int countDMS_ErpDocBarcodes = 0;
                aSqlCommand.CommandText = @"select COUNT(*) from DMS_ErpDocBarcodes where ErpDocumentID in " + inWhereClause;
                countDMS_ErpDocBarcodes = (int)aSqlCommand.ExecuteScalar();

                int countDMS_IndexesSynchronization = 0;
                aSqlCommand.CommandText = @"select COUNT(*) from DMS_IndexesSynchronization where ErpDocumentID in " + inWhereClause;
                countDMS_IndexesSynchronization = (int)aSqlCommand.ExecuteScalar();

                int countDMS_Attachment = 0;
                aSqlCommand.CommandText = @"select COUNT(*) from DMS_Attachment where ErpDocumentID in " + inWhereClause;
                countDMS_Attachment = (int)aSqlCommand.ExecuteScalar();

                foreach (int id in ids)
                {
                    if (countDMS_IndexesSynchronization > 0)
                    {
                        aSqlCommand.CommandText = @"update DMS_IndexesSynchronization set ErpDocumentID = @PRIMOID where ErpDocumentID=@codToChange";
                        aSqlCommand.Parameters.AddWithValue("@codToChange", id);
                        aSqlCommand.Parameters.AddWithValue("@PRIMOID", PRIMOID);
                        aSqlCommand.ExecuteNonQuery();
                    }
                    if (countDMS_ErpDocBarcodes > 0)
                    {
                        aSqlCommand.CommandText = @"update DMS_ErpDocBarcodes set ErpDocumentID = @PRIMOID where ErpDocumentID=@codToChange";
                        aSqlCommand.Parameters.AddWithValue("@codToChange", id);
                        aSqlCommand.Parameters.AddWithValue("@PRIMOID", PRIMOID);
                        aSqlCommand.ExecuteNonQuery();
                    }
                    if (countDMS_Attachment > 0)
                    {
                        aSqlCommand.CommandText = @"update DMS_Attachment set ErpDocumentID = @PRIMOID where ErpDocumentID=@codToChange";
                        aSqlCommand.Parameters.AddWithValue("@codToChange", id);
                        aSqlCommand.Parameters.AddWithValue("@PRIMOID", PRIMOID);
                        aSqlCommand.ExecuteNonQuery();
                    }
                    DeleteDuplicatedErpDocID(id);
                }
            }
        }

        //----------------------------------------------------------------------
        private void DeleteDuplicatedErpDocID(int val)
        {
            try
            {
                using (SqlConnection myConnection = new SqlConnection(DMSOrchestrator.DMSConnectionString))
                {
                    myConnection.Open();
                    using (SqlCommand aSqlCommand = myConnection.CreateCommand())
                    {
                        aSqlCommand.Connection = myConnection;
                        aSqlCommand.CommandText = "DELETE from DMS_ErpDocument where ErpDocumentID = @ID";
                        aSqlCommand.Parameters.AddWithValue("@ID", val);
                        aSqlCommand.ExecuteNonQuery();

                    }
                }
            }
            catch (Exception exc)
            {
                SetMessage(string.Format(Strings.Error, exc.Message), exc, "DeleteDuplicatedErpDocID");
            }

        }

    }


    ///<summary>
	/// DocumentManagement class
	///</summary>
	//================================================================================
    public class AttachManager : ArchiveManager
    {      
        // Events
        public event EventHandler<AttachmentInfoEventArgs> AttachCompleted;

        public DMSDocOrchestrator DMSDocOrchestrator;


        //---------------------------------------------------------------------
		public AttachManager(DMSDocOrchestrator dmsOrchestrator)
		: base(dmsOrchestrator)
        {
            DMSDocOrchestrator = dmsOrchestrator;
			ManagerName = "AttachManager";

			DMSDocOrchestrator.DeleteErpDocument += new EventHandler(dmsOrchestrator_DeleteErpDocument);   
		}

        //---------------------------------------------------------------------
        private void AddXMLSearchBookmarks(ref BookmarksDataTable dataTable)
        {
            // I load the data about the SearchBookmark in DBTS.xml document description file
            // these information are used to :
            //  compose the description of the erp document in search results
            //  reach auxiliary data using hotlink. I.e. companyname, taxcode... about a customer in invoice document (see SOSConnector improvement #5062)
            //  add new field		
            try
            {
                if (DMSDocOrchestrator.Bookmarks != null)
                {
                    foreach (MXMLSearchBookmark bookmark in DMSDocOrchestrator.Bookmarks)
                        dataTable.AddXMLSearchBookmark(bookmark, DMSDocOrchestrator);
                }
                //if no ShowAsDescription field exist I look for "Description" table field
                DataRow[] descriRows = dataTable.Select(string.Format("{0} = true", CommonStrings.ShowAsDescription));
                if (descriRows == null || descriRows.Count() == 0)
                {
                    MSqlRecordItem recItem = (MSqlRecordItem)DMSDocOrchestrator.MasterRecord.GetField("Description");
                    if (recItem != null)
                    {
                        DataRow[] rows = dataTable.Select(string.Format("{0} = '{1}'", CommonStrings.Name, "Description"));
                        if (rows.Count() > 0)
                            rows.Single()[CommonStrings.ShowAsDescription] = true;
                        else
                            dataTable.AddBindingField(recItem, true);
                    }
                }
            }
            catch (Exception e)
            {
                throw (e);
            }
        }

        //---------------------------------------------------------------------
        public BookmarksDataTable CreateStandardTemplate(DMS_Collection collection)
        {
            BookmarksDataTable dataTable = new BookmarksDataTable();

            try
            {
                AddDocumentBookmarks(ref dataTable);
                collection.Version = DMSDocOrchestrator.CurrentCollectionVersion;

                if (DMSDocOrchestrator.DocumentNamespaceInSos)
                {
                    collection.SosDocClass = DMSOrchestrator.SOSConfigurationState.DocumentClasses.GetInternalDocClass(DMSDocOrchestrator.DocumentNamespace);
                    sosManager.AdjustTemplateForSosConnector(ref dataTable);
                }
            }
            catch (Exception e)
            {
                throw (e);
            }

            return dataTable;
        }

        //---------------------------------------------------------------------
        private void AddDocumentBookmarks(ref BookmarksDataTable dataTable)
        {
            //prima verifico che ci sia il DBTMaster
            if (DMSDocOrchestrator.MasterRecord != null)
            {
                IList recFields = DMSDocOrchestrator.MasterRecord.GetFieldsNoExtensions();
                foreach (MSqlRecordItem recItem in recFields)
                {
					if (recItem.IsSegmentKey && !(recItem is MLocalSqlRecordItem))
                        dataTable.AddBindingField(recItem, false);
                }
                // I load the data about the SearchBookmark in DBTS.xml document description file
                AddXMLSearchBookmarks(ref dataTable);
            }

            
            //se il documento è di tipo batch 
            //vado a considerare le variabili di documento che devono essere prese come bookmark(quelle presenti nella CVariableArray del CDMSAttachmentManagerObj)
            //NOTA: prima erano di documento ma in TBF le variabili presenti in m_pXMLVariableArray sono tutti i datamember di documento e non più solo quelli del DMS
            //questo poichè queste variabili sono utilizzati dal motore json
            if (DMSDocOrchestrator.Document.BookmarkXMLVariables != null)
                foreach (MXMLVariable variable in DMSDocOrchestrator.Document.BookmarkXMLVariables.XMLVariables)
                    dataTable.AddBindingField(variable);
        }
        
        //---------------------------------------------------------------------		
        public List<string> GetBookmarkFieldsToObserve(int collectionID)
        {
            List<string> bookmarkFieldsToObserve = searchManager.GetBookmarkFieldsToObserve(collectionID);

            //se non esistono bookmark specifici per il documento passato vuol dire che sto effettuando il primo attachment
            // in questo caso considero il template di default

            if (bookmarkFieldsToObserve.Count == 0 && defaultBookmarksDT != null)
                foreach (DataRow row in defaultBookmarksDT.Rows)
                    bookmarkFieldsToObserve.Add(row[CommonStrings.Name].ToString());

            return bookmarkFieldsToObserve;
        }

        //---------------------------------------------------------------------
        public BookmarksDataTable GetDefaultCollectionFields(DMS_Collection collection)
        {
            BookmarksDataTable dataTable = new BookmarksDataTable();
            IQueryable<DMS_CollectionsField> fields = null;

            //devo aggiornare il template per cui carico tutti i campi altrimenti carico solo i campi attivi
            bool toBeUpdate = (collection.Version < DMSDocOrchestrator.CurrentCollectionVersion ||
                                (DMSDocOrchestrator.DocumentNamespaceInSos && string.IsNullOrEmpty(collection.SosDocClass)));
            try
            {
                if (toBeUpdate)
                    fields = from coll in dc.DMS_CollectionsFields
                             where coll.CollectionID == collection.CollectionID
                             select coll;
                else
                    fields = from coll in dc.DMS_CollectionsFields
                             where coll.CollectionID == collection.CollectionID
                             && coll.Disabled == false
                             select coll;

                if ((fields == null || !fields.Any()) && collection.Name.CompareTo(CommonStrings.EACollection) != 0)
                {
                    //the template does't exist. I create it.
                    AddDocumentBookmarks(ref dataTable);

                    collection.Version = DMSDocOrchestrator.CurrentCollectionVersion;

                    if (DMSDocOrchestrator.DocumentNamespaceInSos && sosManager.AdjustTemplateForSosConnector(ref dataTable))
                        collection.SosDocClass = DMSDocOrchestrator.SOSConfigurationState.DocumentClasses.GetInternalDocClass(DMSDocOrchestrator.DocumentNamespace);

                    SaveStandardTemplate(collection, dataTable);
                    SyncronizeFieldsValue(dataTable);
                }
                else
                {
                    dataTable.Fill(fields, DMSDocOrchestrator);
                    if (toBeUpdate)
                    {
                        dataTable.AcceptChanges();
                        //I must update the template if collection has a version < of version in dbts.xml file
                        //or if the document is in sos but 
                        if (collection.Version < DMSDocOrchestrator.CurrentCollectionVersion)
                        {
                            // I load the data about the SearchBookmark in DBTS.xml document description file
                            AddXMLSearchBookmarks(ref dataTable);
                            collection.Version = DMSDocOrchestrator.CurrentCollectionVersion;
                        }

                        if (DMSDocOrchestrator.DocumentNamespaceInSos && string.IsNullOrEmpty(collection.SosDocClass) && sosManager.AdjustTemplateForSosConnector(ref dataTable))
                            collection.SosDocClass = DMSDocOrchestrator.SOSConfigurationState.DocumentClasses.GetInternalDocClass(DMSDocOrchestrator.DocumentNamespace);

                        SaveStandardTemplate(collection, dataTable);
                    }
                    SyncronizeFieldsValue(dataTable);
                    dataTable.AcceptChanges();
                   
                }
            }
            catch (Exception e)
            {
                throw (e);
            }
            return dataTable;
        }

        /// <summary>
        /// used to syncronize the datafield value with the ERP current browsed document
        /// </summary>
        //---------------------------------------------------------------------
        override public void SyncronizeFieldsValue(BookmarksDataTable fields)
        {
            try
            {
                if (fields == null)
                    return;

                for (int i = 0; i < fields.Rows.Count; i++)
                {
                    DataRow row = fields.Rows[i];
                    // se non e' specificato un nome nel grid non eseguo la sincronizzazione del field
                    if (
                            row != null &&
                            !string.IsNullOrWhiteSpace(row[CommonStrings.Name].ToString())
                        )
                        SyncronizeFieldValue(ref row);
                }
                fields.AcceptChanges();
            }
            catch (Exception e)
            {
                SetMessage(Strings.ErrorSynchronizingField, e, "SyncronizeFieldsValue");
            }
        }

        //sincronizzo solo i bookmark di tipo external
        //---------------------------------------------------------------------
        public void SyncronizeExternalFieldsValue(BookmarksDataTable fields)
        {
            try
            {
                if (fields == null)
                    return;

                for (int i = 0; i < fields.Rows.Count; i++)
                {
                    DataRow row = fields.Rows[i];
                    // se non e' specificato un nome nel grid non eseguo la sincronizzazione del field
                    if (
                            row != null &&
                            (FieldGroup)row[CommonStrings.FieldGroup] == FieldGroup.External &&
                            !string.IsNullOrWhiteSpace(row[CommonStrings.Name].ToString())
                        )
                        SyncronizeFieldValue(ref row);
                }
                fields.AcceptChanges();
            }
            catch (Exception e)
            {
                SetMessage(Strings.ErrorSynchronizingField, e, "SyncronizeFieldsValue");
            }
        }

        //---------------------------------------------------------------------
        public void SyncronizeFieldValue(ref DataRow row)
        {
            string fieldName = row[CommonStrings.Name].ToString();
            switch ((FieldGroup)row[CommonStrings.FieldGroup])
            {
                case FieldGroup.Key:
                case FieldGroup.Binding:
                    MSqlRecordItem recField = null;
                    /*if ((FieldGroup)row[CommonStrings.FieldGroup] == FieldGroup.External)
                        recField = (row[CommonStrings.HotKeyLink] == DBNull.Value) ? null : (MSqlRecordItem)(((MHotLink)row[CommonStrings.HotKeyLink]).Record.GetField(fieldName));
                    else*/
                    recField = (MSqlRecordItem)DMSDocOrchestrator.MasterRecord.GetField(fieldName);
                    if (recField != null)
                        ((FieldData)row[CommonStrings.FieldData]).DataValue = ((MDataObj)recField.DataObj).Value;

                    ((List<FieldData>)row[CommonStrings.ValueSet]).Clear();
                    ((List<FieldData>)row[CommonStrings.ValueSet]).Add((FieldData)row[CommonStrings.FieldData]);

                    break;

				//Bugfix #26240
				case FieldGroup.External:			
					if (string.Compare(fieldName, CommonStrings.CompanyName, true) == 0)
						((FieldData)row[CommonStrings.FieldData]).StringValue = DMSDocOrchestrator.Document.GetCompanyName();
					else
					{
						if (string.Compare(fieldName, CommonStrings.FiscalCode, true) == 0)
							((FieldData)row[CommonStrings.FieldData]).StringValue = DMSDocOrchestrator.Document.GetFiscalCode();
						else
							if (string.Compare(fieldName, CommonStrings.TaxIdNumber, true) == 0)
							((FieldData)row[CommonStrings.FieldData]).StringValue = DMSDocOrchestrator.Document.GetTaxIdNumber();
					}
					break;

				case FieldGroup.SosSpecial:
                    //se campo =  FiscalYear per il valore devo chiamare la funzione virtuale del documento specifica del campo
                    if (string.Compare(fieldName, CommonStrings.FiscalYear, true) == 0)
                    {
                        int year = DMSDocOrchestrator.Document.GetFiscalYear();
                        ((FieldData)row[CommonStrings.FieldData]).StringValue = year.ToString();
                        Debug.Assert(year != DateTimeFunctions.MinYear);
                    }
                    else
                    {
                        if (string.Compare(fieldName, CommonStrings.Suffix, true) == 0)
                            ((FieldData)row[CommonStrings.FieldData]).StringValue = DMSDocOrchestrator.Document.GetSosSuffix();
                        // commentato: da ripristinare nel caso di contribuenti diversi con SOS
                        /*if (string.Compare(fieldName, CommonStrings.CodSog, true) == 0)
                            ((FieldData)row[CommonStrings.FieldData]).StringValue = DMSOrchestrator.SOSSubjectCode;*/
                        else
                            if (string.Compare(fieldName, CommonStrings.SOSDocumentType, true) == 0)
                                ((FieldData)row[CommonStrings.FieldData]).StringValue = DMSDocOrchestrator.GetSOSDocumentType();

                    }

                    break;

                case FieldGroup.Category:
                    ((FieldData)row[CommonStrings.FieldData]).StringValue = categoryManager.GetCategoryDefaultValue(row[CommonStrings.Name].ToString());
                    row[CommonStrings.ValueSet] = categoryManager.GetCategoryValues(row[CommonStrings.Name].ToString());
                    break;

                case FieldGroup.Variable:
                    MXMLVariableArray variables = DMSDocOrchestrator.Document.BookmarkXMLVariables;
                    MXMLVariable variable = variables.GetVariable(row[CommonStrings.Name].ToString());
                    ((FieldData)row[CommonStrings.FieldData]).StringValue = (variable != null) ? variable.DataObj.StringValue : string.Empty;
                    ((List<FieldData>)row[CommonStrings.ValueSet]).Clear();
                    ((List<FieldData>)row[CommonStrings.ValueSet]).Add((FieldData)row[CommonStrings.FieldData]);
                    break;
            }
            row[CommonStrings.FormattedValue] = ((FieldData)row[CommonStrings.FieldData]).FormattedValue;
        }

        //---------------------------------------------------------------------
        public void AttachDocumentInfo(ref AttachmentInfo attachmentInfo)
        {
            if (attachmentInfo == null)
                return;

            try
            {   //then I check if the document is already attached to corrent erp document
                DMS_Attachment attachment = searchManager.GetAttachmentForArchivedDocId
                    (
                    attachmentInfo.ArchivedDocId,
                    DMSDocOrchestrator.DocumentNamespace,
                    DMSDocOrchestrator.DocumentPrimaryKey
                    );

                if (attachment != null)
                {
                    attachmentInfo.Valorize(attachment);
                    if (existingAction == DuplicateDocumentAction.ReplaceExistingDoc)
                    {
                        attachment.TBModified = dc.GetDate();
                        attachment.TBModifiedID = DMSOrchestrator.WorkerId;
                        dc.SubmitChanges();
                        GetBookmarksValues(DMSDocOrchestrator.CollectionID, ref attachmentInfo);
                        //potre aver voluto sostituire il documento per renderlo disponibile alla sostitutiva
                        // 1. il documento di ERP e' in sostitutiva
                        // 2. si tratta di un doc convertibile in PDF/A  
                        // 3. non esiste gia' il record
                        if (DMSDocOrchestrator.DocumentNamespaceInSos)
                        {
                            attachmentInfo.CreateSOSBookmark = true;
                            sosManager.CreateNewSosDocument(ref attachmentInfo);
                        }
                    }
                    if (existingAction != DuplicateDocumentAction.ReplaceExistingDoc && existingAction != DuplicateDocumentAction.UseExistingDoc)
                        SetMessage(string.Format(Strings.AlreadyAttachedFile, attachmentInfo.Name), null, "AttachDocumentInfo");//todo messaggio erroe
                }
                else
                {
                    //set the default value
                    existingAction = DuplicateDocumentAction.Cancel;

                    DMS_Attachment newAttachment = new DMS_Attachment();
                    if (DMSDocOrchestrator.ErpDocumentID == -1)
                        newAttachment.DMS_ErpDocument = DMSDocOrchestrator.ErpDocument;
                    else
                        newAttachment.ErpDocumentID = DMSDocOrchestrator.ErpDocument.ErpDocumentID;
                    //verifico se esiste o meno un papery associato al documento con lo stesso barcode del documento archiviato che sto allegando
                    DMS_ErpDocBarcode erpDocBarcode = DMSDocOrchestrator.BarcodeManager.GetERPDocumentBarcode(attachmentInfo.TBarcode.Value, newAttachment.ErpDocumentID);
                    newAttachment.ArchivedDocID = attachmentInfo.ArchivedDocId;
                    newAttachment.CollectionID = DMSDocOrchestrator.DocumentCollection.CollectionID;
                    newAttachment.Description = (!string.IsNullOrEmpty(attachmentInfo.Description)) ? attachmentInfo.Description : string.Empty;
                    newAttachment.IsMainDoc = (bool)attachmentInfo.IsWoormReport || (bool)attachmentInfo.IsMainDoc || erpDocBarcode != null; //Impr. #6259
					newAttachment.IsForMail = false;


					if (string.IsNullOrEmpty(newAttachment.Description))
                        newAttachment.Description = (!string.IsNullOrEmpty(attachmentInfo.Name)) ? Path.GetFileName(attachmentInfo.Name) : string.Empty;
                    newAttachment.TBCreatedID = DMSOrchestrator.WorkerId;
                    newAttachment.TBModifiedID = DMSOrchestrator.WorkerId;
                    // SOS
                    newAttachment.AbsoluteCode = string.Empty;
                    newAttachment.LotID = string.Empty;
                    newAttachment.RegistrationDate = (DateTime)SqlDateTime.MinValue;
                    dc.DMS_Attachments.InsertOnSubmit(newAttachment);
                    if (erpDocBarcode != null)
                        dc.DMS_ErpDocBarcodes.DeleteOnSubmit(erpDocBarcode);

                    dc.SubmitChanges();

                    if (DMSDocOrchestrator.ErpDocumentID == -1)
                        DMSDocOrchestrator.ErpDocumentID = newAttachment.DMS_ErpDocument.ErpDocumentID;
                    newAttachment.TBModified = dc.GetDate();	//se no la data di modifica è antecedente a quella di creazione
                    dc.SubmitChanges();
                    attachmentInfo.Valorize(newAttachment);
                    if (defaultBookmarksDT == null)
                        defaultBookmarksDT = GetDefaultCollectionFields(DMSDocOrchestrator.DocumentCollection);
                    //per i documenti di tipo batch sincronizzo sempre i valori prima di salvarli sul db, poichè non abbiamo il browsing od il salvataggio del document (azioni che danno origine alla sincronizzazione)
                    //e non è possibile utilizzare OnBeforeBatchExecute poichè non è detto che la richiesta di archiviare venga eseguita
                    if (DMSDocOrchestrator.Document.BookmarkXMLVariables != null)
                        SyncronizeFieldsValue(defaultBookmarksDT);
                    attachmentInfo.BookmarksDT = defaultBookmarksDT;
                    attachmentInfo.Tags = string.Empty;

                    InsertIndexes(attachmentInfo, string.Empty);

                    // procedo ad inserire il documento nel SOSDocument solo se:
                    // 1. il documento di ERP e' in sostitutiva
                    // 2. si tratta di un doc convertibile in PDF/A  
                    // 3. non esiste gia' il record
                    if (DMSDocOrchestrator.DocumentNamespaceInSos)
                        sosManager.CreateNewSosDocument(ref attachmentInfo);
                }

                if (AttachCompleted != null)
                {
                    AttachmentInfoEventArgs arg = new AttachmentInfoEventArgs();
                    arg.CurrentAttachment = attachmentInfo;
                    AttachCompleted(this, arg);
                }
            }
            catch (Exception e)
            {
                SetMessage(string.Format(Strings.ErrorAttachingFile, attachmentInfo.Name), e, "AttachDocumentInfo"); //todo messaggio
            }
        }
        //---------------------------------------------------------------------
        private ArchiveResult AttachDocument(ref DocumentToArchive docToArchive, out int attachmentID)
        {
            attachmentID = -1;
            FileInfo f = new FileInfo(docToArchive.DocumentPath);
            if (
                    (docToArchive.IsAFile && f.Length == 0) ||
                    (!docToArchive.IsAFile && (docToArchive.BinaryContent == null || docToArchive.BinaryContent.Count() == 0))
                )
            {
                SetMessage(String.Format(Strings.FileIsEmpty, docToArchive.DocumentPath), null, "AttachDocument");
                return ArchiveResult.TerminatedWithError;
            }


            //first check if the document is already in repository
            //if it not exists it will be archived first
            try
            {
                DateTime start = DateTime.Now;
                Debug.WriteLine("Start AttachDocument");

                AttachmentInfo attInfo = null;
                bool result = GetArchivedDocumentInfo(out attInfo, ref docToArchive);

                if (attInfo != null)
                {
                    AttachDocumentInfo(ref attInfo);
                    attachmentID = attInfo.AttachmentId;
                    //this.DMSDocOrchestrator.BarcodeManager.DeleteBarcodeForErpDocument(attInfo.TBarcode.Value, this.DMSDocOrchestrator.ErpDocumentID);
                    TimeSpan elapsed = DateTime.Now - start;
                    Debug.WriteLine(string.Format("End AttachDocument in {0}", elapsed.ToString()));
                    return ArchiveResult.TerminatedSuccess;
                }
                else
					return (result && (existingAction == DuplicateDocumentAction.Cancel || existingAction == DuplicateDocumentAction.RefuseAttachmentOperation)) ? ArchiveResult.Cancel : ArchiveResult.TerminatedWithError;
          }
            catch (OutOfMemoryException oute)
            {
                SetMessage(string.Format(Strings.ErrorArchivingTooLargeFile, docToArchive.DocumentPath, "1.2"), oute, "AttachDocument");
            }
            catch (Exception e)
            {
                SetMessage(string.Format(Strings.ErrorAttachingFile, docToArchive.DocumentPath), e, "AttachDocument");
            }

            return ArchiveResult.TerminatedWithError;

        }
        /// <summary>
		/// Used to archive and attach a file to the current ERP document
		/// </summary>
		//---------------------------------------------------------------------
        public ArchiveResult AttachFile(string docPath, string description, out int attachmentID, bool isWoormReport = false, bool skipDetectBarcode = false, string barcode = "")
        {
            attachmentID = -1;
            // click with no selected document
            if (string.IsNullOrWhiteSpace(docPath))
            {
                SetMessage(Strings.ErrorPathIsEmpty, null, "AttachDocument");
                return ArchiveResult.TerminatedWithError;
            }
            DocumentToArchive docToArchive = new DocumentToArchive();
            docToArchive.IsAFile = true;
            docToArchive.DocumentPath = docPath;
            docToArchive.Description = description;
            docToArchive.Barcode = barcode;
            docToArchive.SkipDetectBarcode = skipDetectBarcode;
            docToArchive.IsWoormReport = isWoormReport;

            return AttachDocument(ref docToArchive, out attachmentID);
        }


        /// <summary>
        /// Used to archive and attach a binary content to the current ERP document
        /// </summary>
        //---------------------------------------------------------------------
        public ArchiveResult AttachBinaryContent(byte[] binaryContent, string sourceFileName, string description, out int attachmentId)
        {
            attachmentId = -1;
            // click with no selected document
            if (string.IsNullOrWhiteSpace(sourceFileName))
            {
                SetMessage(Strings.ErrorPathIsEmpty, null, "AttachBinaryContent");
                return ArchiveResult.TerminatedWithError;
            }

            DocumentToArchive docToArchive = new DocumentToArchive();
            docToArchive.IsAFile = false;
            docToArchive.BinaryContent = binaryContent;
            docToArchive.DocumentPath = sourceFileName;
            docToArchive.Description = description;
            docToArchive.IsWoormReport = false;
            docToArchive.Barcode = string.Empty;
            docToArchive.SkipDetectBarcode = false;

            ArchiveResult archResult = AttachDocument(ref docToArchive, out attachmentId);
            docToArchive.BinaryContent = null;
            return archResult;
        }

        //---------------------------------------------------------------------
        public void AttachDocument(ref AttachmentInfo attachmentInfo)
        {
            if (attachmentInfo == null) return;
            AttachDocumentInfo(ref attachmentInfo);
        }

        ///<summary>
        /// Esegue l'attach del documento archiviato passato come parametro 
        /// al documento di ERP corrente ed elimina anche la riga corrispondente
        /// nella tabella DMS_ErpDocBarcodes
        /// N.B. Questo metodo e' richiamato quando voglio allegare un pending papery
        /// individuato da un barcode che identifica un documento gia' archiviato in EA
        ///</summary>
        //---------------------------------------------------------------------
        public int AttachPendingPapery(DMS_ArchivedDocument archiveDoc, int erpDocumentId = -1)
        {
            if (archiveDoc == null)
                return -1;

            AttachmentInfo ai = new AttachmentInfo(archiveDoc, this.DMSOrchestrator);
            if (ai == null)
                return -1;

            AttachDocumentInfo(ref ai);
            //this.DMSDocOrchestrator.BarcodeManager.DeleteBarcodeForErpDocument(archiveDoc.Barcode, this.DMSDocOrchestrator.ErpDocumentID);
            return ai.AttachmentId;
        }

        // I can attach the binary presents in DMS_DocumentToArchive that have the same namespace and key of DMSOrchestrator.Document
        // Linq per evitare di fare roundtrip al database si cache in memoria i dati estratti per ogni entity
        // per evitare questo la gestione dei documenti da archiviare contenuti nella tabella viene fatta utilizzando le 
        // classi del SqlClient 
        //------------------------------------------------------------------------------------------------------------
        public ArchiveResult AttachFromTable()
        {
            ArchiveResult result = ArchiveResult.TerminatedSuccess;
            byte[] contentArray = null;
            string filePath = string.Empty;
            SqlConnection myConnection = new SqlConnection(DMSDocOrchestrator.DMSConnectionString);
            SqlDataAdapter adapter = null;
            try
            {
                myConnection.Open();
                string cmdText = string.Format(
                            "SELECT DocToArchiveID, [Name], [Description], BinaryContent, AttachmentID FROM DMS_DocumentToArchive WHERE DocNamespace = '{0}' AND PrimaryKeyValue = '{1}'",
                            DMSDocOrchestrator.DocumentNamespace,
                            DMSDocOrchestrator.DocumentPrimaryKey);

                adapter = new SqlDataAdapter(cmdText, myConnection);
                adapter.UpdateCommand = new SqlCommand("Update DMS_DocumentToArchive set AttachmentID = @AttachmentID where DocToArchiveID = @DocToArchiveID", myConnection);
                adapter.UpdateCommand.Parameters.Add("@AttachmentID", SqlDbType.Int, 1, "AttachmentID");

                SqlParameter parameter = adapter.UpdateCommand.Parameters.Add("@DocToArchiveID", SqlDbType.Int);
                parameter.SourceColumn = "DocToArchiveID";
                parameter.SourceVersion = DataRowVersion.Original;

                DataTable table = new DataTable();
                adapter.Fill(table);

                foreach (DataRow row in table.Rows)
                {
                    filePath = Path.Combine(DMSDocOrchestrator.DocToArchiveTempPath, row["Name"].ToString());

                    contentArray = (byte[])row["BinaryContent"];
                    int attachmentID;
                    if (
                        AttachBinaryContent(contentArray, filePath, row["Description"].ToString(), out attachmentID) == ArchiveResult.TerminatedSuccess &&
                        attachmentID > -1
                        )
                        row["AttachmentID"] = attachmentID;
                    else
                        result = ArchiveResult.TerminatedWithError;

                    contentArray = null;
                }
                adapter.Update(table);
            }
            catch (SqlException e)
            {
                SetMessage(Strings.ErrorAttachingFromTable, e, "AttachFromTable");
                result = ArchiveResult.TerminatedWithError;
            }
            catch (OutOfMemoryException oute)
            {
                contentArray = null;
                SetMessage(string.Format(Strings.ErrorArchivingTooLargeFile, filePath, "1.2"), oute, "AttachFromTable");
                result = ArchiveResult.TerminatedWithError;
            }
            catch (Exception ex)
            {
                SetMessage(Strings.ErrorAttachingFromTable, ex, "AttachFromTable");
                result = ArchiveResult.TerminatedWithError;
            }
            finally
            {
                adapter.Dispose();
                if (myConnection.State == ConnectionState.Open || myConnection.State == ConnectionState.Broken)
                    myConnection.Close();
            }

            return result;
        }
     

    
        // when the ErpDocument is removed I have to remove also all its attachment 
        //---------------------------------------------------------------------
        void dmsOrchestrator_DeleteErpDocument(object sender, EventArgs e)
        {
            if (DMSDocOrchestrator.ErpDocumentID == -1)
                return;
            string strError = string.Empty;
            try
            {
                DeleteErpDocument(DMSDocOrchestrator.ErpDocumentID);
                DMSDocOrchestrator.ErpDocument = null;
            }
            catch (Exception ex)
            {
                SetMessage(Strings.ErrorDeletingAttachment, ex, "DeleteErpDocument");
            }

        }

        //---------------------------------------------------------------------
        internal ArchiveResult AttachPapery(string barcodeValue, string notes, string fileName)
        {
            AttachmentInfo paperyAttInfo = barcodeManager.GetAttachmentInfoFromBarcode(barcodeValue, notes, DMSDocOrchestrator.ErpDocumentID);
            if (paperyAttInfo == null)
                return ArchiveResult.TerminatedWithError;

            if (paperyAttInfo.IsAPapery)
            {
                if (!this.barcodeManager.UpdatePapery(barcodeValue, notes, fileName, DMSDocOrchestrator)) // eseguo l'insert o l'update del pending papery 
	                 return ArchiveResult.TerminatedWithError;
            }
            else
            {
                // devo inserire l'attachment
                if (paperyAttInfo.AttachmentId == -1)
                {
                    AttachDocumentInfo(ref paperyAttInfo);
                    return ArchiveResult.TerminatedSuccess; // ritorno per non eseguire due volte l'AttachCompleted
                }
            }

            if (AttachCompleted != null)
            {
                AttachmentInfoEventArgs arg = new AttachmentInfoEventArgs();
                arg.CurrentAttachment = paperyAttInfo;
                AttachCompleted(this, arg);
            }

            return ArchiveResult.TerminatedSuccess;
        }

        //---------------------------------------------------------------------
        public bool UpdateAttachment(ref AttachmentInfo attachmentInfo, string newDescription, string newTags, bool isMainDoc, bool isForMail)
        {
            if (attachmentInfo.IsAPapery)
                return true;

            if (attachmentInfo == null || attachmentInfo.AttachmentId < 0)
                return false;

            //check if something have been changed
            int attachmentID = attachmentInfo.AttachmentId;
            //check if it exists the attachment
            var var = (from att in dc.DMS_Attachments
                       where att.AttachmentID == attachmentID
                       select att);

            DMS_Attachment attachment = null;

            try
            {
                attachment = (var != null && var.Any()) ? (DMS_Attachment)var.Single() : null;

                if (attachment == null)
                    return false;

                string lockMsg = string.Empty;

                if (DMSOrchestrator.LockManager.LockRecord(attachment, DMSDocOrchestrator.LockContext, ref lockMsg))
                {
                    // if bookmarks are changed I have to update the search indexes and collection template	
                    if (
                            string.Compare(attachmentInfo.Tags, newTags, StringComparison.InvariantCultureIgnoreCase) != 0 ||
                            string.Compare(attachmentInfo.Description, newDescription, StringComparison.InvariantCultureIgnoreCase) != 0 ||
                            attachmentInfo.IsMainDoc != isMainDoc ||
							attachmentInfo.IsForMail != isForMail ||
							attachmentInfo.BookmarksDT.GetChanges() != null
                        )
                    {
                        UpdateSearchIndexes(attachmentInfo, newTags, newDescription);                     
						if (attachmentInfo.BookmarksDT.GetChanges() != null)
                        {
                            SaveStandardTemplate(DMSDocOrchestrator.DocumentCollection, attachmentInfo.BookmarksDT);
                            searchManager.GetBookmarksValues(ref attachmentInfo);
                            attachmentInfo.BookmarksDT.AcceptChanges();
                        }

                        //different description or different isMainDoc
                        if (
								string.Compare(attachmentInfo.Tags, newTags, StringComparison.InvariantCultureIgnoreCase) != 0 ||
								string.Compare(attachment.Description, newDescription, StringComparison.InvariantCultureIgnoreCase) != 0 ||
								attachmentInfo.IsMainDoc != isMainDoc ||
								attachmentInfo.IsForMail != isForMail)
                        {
                            attachment.Description = newDescription;
							attachmentInfo.Tags = newTags;
							attachment.IsMainDoc = isMainDoc;
							attachment.IsForMail = isForMail;
							dc.SubmitChanges();
                        }
                    }
                    DMSOrchestrator.LockManager.UnlockRecord(attachment, DMSOrchestrator.LockContext);
                }
                else
                {
                    SetMessage(string.Format(Strings.ErrorUpdatingAttachment, attachmentInfo.AttachmentId.ToString()), new Exception(lockMsg), "UpdateAttachment");
                    return false;
                }
            }
            catch (Exception e)
            {
                SetMessage(string.Format(Strings.ErrorUpdatingAttachment, attachmentInfo.AttachmentId.ToString()), e, "UpdateAttachment");
                return false;
            }
            return true;
        }

        

		
    }
}
