using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Transactions;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.TbHermesBL.tbDms;

namespace Microarea.TaskBuilderNet.TbHermesBL
{
	public class EasyAttachmentManager : IEasyAttachmentManager, ILockerClient
	{
		TbDMS tbDms;
		DmsDatabaseInfo dmsDB;
		LoginManagerConnector loginManagerConnector;

		//------------------------------------------------------------------------
		public EasyAttachmentManager(
			LoginManagerConnector loginManagerConnector, 
			TbSenderDatabaseInfo cmp
			)
		{
			this.loginManagerConnector = loginManagerConnector;
			this.dmsDB = loginManagerConnector.GetDMSDatabasesInfo4Company(cmp.CompanyId);
			this.DmsDbFound = this.dmsDB != null;
			if (this.dmsDB == null)
				return; // TODO LOG - no è possibile salvare allegati

			string company = cmp.Company;
			SqlConnectionStringBuilder cb = new SqlConnectionStringBuilder(dmsDB.DMSConnectionString);
			if (cb.IntegratedSecurity)
				throw new Exception("DMS db is using Integrated Security"); // TODO

			TbLoaderClientInterface tbCI = loginManagerConnector.GetTbLoaderClientInterface(company, cb);
			TbDMS tbDms = GetTbDMS(tbCI);
			this.tbDms = tbDms;

			// per parte client di LockManager
			this.AuthenticationToken = tbCI.AuthenticationToken;
			this.UserName = cb.UserID;
			this.lockManager = new LockManager(BasePathFinder.BasePathFinderInstance.LockManagerUrl);
		}

		// necessari per LockManager
		private string AuthenticationToken { get; set; }
		private string UserName { get; set; }
		LockManager lockManager;
		const string lockAddr = "TbHermes";

		//------------------------------------------------------------------------
		private TbDMS GetTbDMS(TbLoaderClientInterface tbCI)
		{
			string tbDmsUrlMask = "http://{0}:{1}/Extensions.EasyAttachment.TbDMS/TbDMS";
			string tbDmsUrl = string.Format(CultureInfo.InvariantCulture, tbDmsUrlMask, tbCI.TbServer, tbCI.TbPort);
			TbDMS tbDms = new TbDMS();
			tbDms.Url = tbDmsUrl;
			tbDms.HeaderInfo = new TBHeaderInfo();
			tbDms.HeaderInfo.AuthToken = tbCI.AuthenticationToken;
			return tbDms;
		}

		//------------------------------------------------------------------------
		public string AttachResultText { get; private set; }
		public bool DmsDbFound { get; private set; }

		public List<DMS_DocumentToArchive> StoreEMailAttachments(Limilabs.Mail.IMail mailItem, int officeMailMessageID)
		{
			// OK + ottieni string di connessione a DB EasyAttachment DMS
			//   dovresti beccarla una volta all'inizio e poi va sempre bene, dovrebbe fornirtela LoginManager
			//   List<DmsDatabaseInfo> dmsList = loginManager.GetDMSDatabasesInfo("{2E8164FA-7A8B-4352-B0DC-479984070507}");
			//   (copiato da EASyncEngine.cs)
			//
			// + Lock con LockManager
			//
			// OK + Scrivere record in tabella temporanea

			string primaryKeyValue = GetPrimaryKeyValue(officeMailMessageID);

			//Dictionary<string, DMS_DocumentToArchive> d2aDic = new Dictionary<string,DMS_DocumentToArchive>(StringComparer.InvariantCultureIgnoreCase);
			List<DMS_DocumentToArchive> d2aList = new List<DMS_DocumentToArchive>();
			using (MZPcmpDMSEntities dbDms = ConnectionHelper.GetDmsEntities(this.dmsDB))
			using (TransactionScope tsDms = new TransactionScope(TransactionScopeOption.RequiresNew)) // transazione indipendente perché deve essere letta committata da EasyAttachment
			{
				foreach (var att in mailItem.Attachments)
				{
					DMS_DocumentToArchive d2a = new DMS_DocumentToArchive();
					d2a.DocNamespace = docNamespaceOnDB;
					d2a.PrimaryKeyValue = primaryKeyValue;
					d2a.Name = att.FileName;
                    if (d2a.Name.IsNullOrEmpty())
                        d2a.Name = "";

					d2a.Description = "TODO description";
					d2a.BinaryContent = att.Data;
                    
                    d2a.RowKey = "";
                    d2a.DocToArchiveID = 0;
                    d2a.AttachmentID = 0;

                    //d2a.AttachmentID variabile in out, se serve
                    dbDms.DMS_DocumentToArchive.Add(d2a);
					d2aList.Add(d2a);
					//d2aDic[att.ContentId] = d2a;
				}

                //PIPPO
                //foreach (var att in mailItem.Visuals)
                //{
                //    DMS_DocumentToArchive d2a = new DMS_DocumentToArchive();
                //    d2a.DocNamespace = docNamespaceOnDB;
                //    d2a.PrimaryKeyValue = primaryKeyValue;
                //    d2a.Name = att.FileName;
                //    d2a.Description = "TODO description";
                //    d2a.BinaryContent = att.Data;
                //    //d2a.AttachmentID variabile in out, se serve
                //    dbDms.AddToDMS_DocumentToArchive(d2a);
                //    d2aList.Add(d2a);
                //    //d2aDic[att.ContentId] = d2a;
                //}

				dbDms.SaveChanges();
				tsDms.Complete();
			}

			return d2aList;
		}

        const string docNamespace = "OFM.Mail.Documents.MailMessages";
        const string docNamespaceOnDB = "Document.OFM.Mail.Documents.MailMessages";

		private static string GetPrimaryKeyValue(int officeMailMessageID)
		{
			return "MailMessageID:" + officeMailMessageID.ToString(CultureInfo.InvariantCulture) + ";";
		}

		public List<int?> AttachThem(Limilabs.Mail.IMail mailItem, int officeMailMessageID, List<DMS_DocumentToArchive> d2aList)
		{
			string primaryKeyValue = GetPrimaryKeyValue(officeMailMessageID);
			bool resB = this.Attach(docNamespace, primaryKeyValue);
			if (false == resB || false == string.IsNullOrEmpty(this.AttachResultText))
			{
				throw new Exception(this.AttachResultText); // TODO
			}

			List<int?> idsList = new List<int?>(d2aList.Count); // non mi piace troppo, ma voglio disaccoppiare

			// i dati in canna non sono aggiornati, devi rileggerli da db
			Dictionary<int, DMS_DocumentToArchive> dic = new Dictionary<int, DMS_DocumentToArchive>();
			using (MZPcmpDMSEntities dbDms = ConnectionHelper.GetDmsEntities(this.dmsDB))
			using (TransactionScope tsDms = new TransactionScope(TransactionScopeOption.RequiresNew)) // transazione indipendente
			{
				var items = from d2a in dbDms.DMS_DocumentToArchive
							where d2a.DocNamespace == docNamespaceOnDB && d2a.PrimaryKeyValue == primaryKeyValue
							select d2a;
				// nota: per motivi di concorrenza con altri client potrei averne tirato su anche qualcuno in più, poco importa ora
				foreach (DMS_DocumentToArchive d2a in items)
					dic[d2a.DocToArchiveID] = d2a;

				List<DMS_DocumentToArchive> temp = new List<DMS_DocumentToArchive>(d2aList.Count);
				// mi metto da parte gli id che mi servono
				foreach (DMS_DocumentToArchive d2a in d2aList)
				{
					DMS_DocumentToArchive d2a2 = dic[d2a.DocToArchiveID];
					idsList.Add(d2a2.AttachmentID);
					temp.Add(d2a2); // vi metto questi che sono "attached" con stato aggiornato al db
				}

				// + purge di record temporanei su db di DMS -- Lock con LockManager (anche su EA?)
				try
				{
					foreach (DMS_DocumentToArchive d2a in temp)
					{
						//dbDms.DMS_DocumentToArchive.Attach(d2a);
                        dbDms.DMS_DocumentToArchive.Remove(d2a); //dbDms.DeleteObject(d2a);       
					}
					dbDms.SaveChanges();
					tsDms.Complete();
				}
				catch (Exception ex)
				{
					// TODO LOG, e poi non fare nulla perché alla peggio rimangono dei cadaverini nella tabella temporanea
					System.Diagnostics.Debug.WriteLine(ex.Message);
				}
			}

			return idsList;
		}

		//------------------------------------------------------------------------
		private bool Attach(string documentNamespace, string primaryKeyValue)
		{
			this.AttachResultText = null;
			string resS = null; // cannot use the property as ref param
			bool resB = tbDms.AttachFromTable(documentNamespace, primaryKeyValue, ref resS);
			this.AttachResultText = resS;
			return resB;
		}

		public byte[] GetAttachmentData(OM_MailMessageAttachments attRec)
		{
			int archDocId = attRec.MailEasyAttachmentID ?? -1;
			if (archDocId == -1)
				return null;
			using (MZPcmpDMSEntities dbDms = ConnectionHelper.GetDmsEntities(this.dmsDB))
			//using (TransactionScope tsDms = new TransactionScope(TransactionScopeOption.RequiresNew)) // transazione indipendente
			{
				var item = (from ac in dbDms.DMS_ArchivedDocContent 
							join at in dbDms.DMS_Attachment
							on ac.ArchivedDocID equals at.ArchivedDocID 
							where at.AttachmentID == archDocId
							select ac)
							.FirstOrDefault();
				if (item != null)
					return item.BinaryContent;
			}
			return null;
		}

        public byte[] GetAttachmentData(int? attID)
        {
            int archDocId = attID ?? -1;
            if (archDocId == -1)
                return null;
            using (MZPcmpDMSEntities dbDms = ConnectionHelper.GetDmsEntities(this.dmsDB))
            //using (TransactionScope tsDms = new TransactionScope(TransactionScopeOption.RequiresNew)) // transazione indipendente
            {
                var item = (from ac in dbDms.DMS_ArchivedDocContent
                            join at in dbDms.DMS_Attachment
                            on ac.ArchivedDocID equals at.ArchivedDocID
                            where at.AttachmentID == archDocId
                            select ac)
                            .FirstOrDefault();
                if (item != null)
                    return item.BinaryContent;
            }
            return null;
        }

		//---------------------------------------------------------------------
		bool ILockerClient.LockRecord(string companyDBName, string tableName, string lockKey)
		{
			string authToken = this.AuthenticationToken;
			string userName = this.UserName;

			return lockManager.LockRecord(companyDBName, authToken, userName, tableName, lockKey, lockAddr, lockAddr);
		}

		void ILockerClient.UnlockRecord(string companyDBName, string tableName, string entity)
		{
			string authToken = this.AuthenticationToken;
			lockManager.UnlockRecord(companyDBName, authToken, tableName, entity, lockAddr);
		}

		void IDisposable.Dispose()
		{
			if (this.loginManagerConnector != null)
				this.loginManagerConnector.LogOff();
		}
	}

	public interface IEasyAttachmentManager : IDisposable
	{
		List<DMS_DocumentToArchive> StoreEMailAttachments(Limilabs.Mail.IMail mailItem, int officeMailMessageID);
		List<int?> AttachThem(Limilabs.Mail.IMail mailItem, int officeMailMessageID, List<DMS_DocumentToArchive> d2aList);
		//bool Attach(string documentNamespace, string primaryKeyValue);
		string AttachResultText { get; }
		bool DmsDbFound { get; }
		byte[] GetAttachmentData(OM_MailMessageAttachments attRec);
        byte[] GetAttachmentData(int? attID);
	}

	public class DummyEasyAttachmentManager : IEasyAttachmentManager, ILockerClient
	{
		List<DMS_DocumentToArchive> IEasyAttachmentManager.StoreEMailAttachments(Limilabs.Mail.IMail mailItem, int officeMailMessageID)
		{
			int cnt = mailItem.Attachments.Count;
			List<DMS_DocumentToArchive> list = new List<DMS_DocumentToArchive>(cnt);
			for (int i = 0; i < cnt; ++i)
				list.Add(new DMS_DocumentToArchive());
			return list;
		}
		List<int?> IEasyAttachmentManager.AttachThem(Limilabs.Mail.IMail mailItem, int officeMailMessageID, List<DMS_DocumentToArchive> d2aList)
		{
			int cnt = mailItem.Attachments.Count;
			List<int?> list = new List<int?>(cnt);
			for (int i = 0; i < cnt; ++i)
				list.Add(-1);
			return list;
		}

		string IEasyAttachmentManager.AttachResultText
		{
			get { return null; }
		}

		bool IEasyAttachmentManager.DmsDbFound
		{
			get { return true; }
		}

		byte[] IEasyAttachmentManager.GetAttachmentData(OM_MailMessageAttachments attRec)
		{
			return null;
		}

        byte[] IEasyAttachmentManager.GetAttachmentData(int? attID)
        {
            return null;
        }

		bool ILockerClient.LockRecord(string companyDBName, string tableName, string lockKey)
		{
			return true;
		}

		void ILockerClient.UnlockRecord(string companyDBName, string tableName, string lockKey)
		{
			// nop
		}

		void IDisposable.Dispose()
		{
			// nop
		}
	}
}
