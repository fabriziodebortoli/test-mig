using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.EasyAttachment.Components;
using System.IO;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;


namespace Microarea.EasyAttachment.BusinessLogic
{
	//================================================================================
	class ERPDocumentManager : BaseManager
	{
		///members
		//-----------------------------------------------------------------------------------------------
		private TBConnection tbConnection = null;
		private TBCommand tbCommand = null;
		private string docNamespace = string.Empty;
		private MDocument erpDocument = null;
		private string oldPrimaryKey = string.Empty;

		private Diagnostic documentDiagnostic = new Diagnostic("ERPDocumentManager");
		///properties
		//-----------------------------------------------------------------------------------------------
		private MSqlRecord MasterRecord { get { return (erpDocument != null) ? (MSqlRecord)erpDocument.Master.Record : null; } }

		//--------------------------------------------------------------------------------
		public int DocumentHandle { get { return erpDocument.TbHandle; } }

		public Diagnostic DocumentDiagnostic {  get { return documentDiagnostic; } }

		//-----------------------------------------------------------------------------------------------		
		public ERPDocumentManager(string dNamespace)
		{
			docNamespace = dNamespace;
		}

		//-----------------------------------------------------------------------------------------------
		private TBCommand PrepareCommand(BookmarksDataTable fieldsDT)
		{
			TBCommand command = new TBCommand(tbConnection);
			string cmdText = string.Empty;
			string select = string.Empty;
			string filter = string.Empty;
			string field = string.Empty;


			IList recFields = MasterRecord.GetFieldsNoExtensions();
			foreach (MSqlRecordItem recItem in recFields)
			{
				if (recItem.IsSegmentKey && !(recItem is MLocalSqlRecordItem))
				{
					if (!string.IsNullOrWhiteSpace(select))
						select += ", ";
					select += recItem.Name;
				}
			}

			string param = string.Empty;
			int i = 0;
			try
			{
				foreach (DataRow row in fieldsDT.Rows)
				{
					++i;
					field = row[CommonStrings.PhysicalName].ToString();
					field = field.Substring(field.IndexOf('.') + 1);
					param = string.Format("@param{0}", i.ToString());
					if (!string.IsNullOrWhiteSpace(filter))
						filter += " AND ";
					filter += string.Format("{0} = {1}", field, param);
					command.Parameters.Add(param, row[CommonStrings.Value]);
				}

				command.CommandText = string.Format(@"SELECT {0} FROM {1} WHERE {2}", select, MasterRecord.Name, filter);
			}
			catch (TBException err)
			{
				throw (err);
			}

			return command;
		}

		//-----------------------------------------------------------------------------------------------
		private void SetParametersValue(BookmarksDataTable fieldsDT)
		{
			if (tbCommand == null || fieldsDT.Rows.Count != tbCommand.Parameters.Count)
				return;

			TBParameter param = null;
			DataRow row = null;

			try
			{
				for (int i = 0; i < tbCommand.Parameters.Count; i++)
				{
					param = tbCommand.Parameters.GetParameterAt(i);
					row = fieldsDT.Rows[i];
					param.Value = row[CommonStrings.Value];
				}
			}
			catch (TBException err)
			{
				throw (err);
			}
		}

		//-----------------------------------------------------------------------------------------------
		public bool OpenDocument()
		{
			try
			{
				erpDocument = MDocument.CreateUnattended<MDocument>(docNamespace, null);
				return erpDocument != null;
			}
			catch (TBException err)
			{
				SetMessage("An error occurred opening ERP document", err, "OpenDocument");
				return false;
			}
		}

		//-----------------------------------------------------------------------------------------------
		public bool FindDocument(BookmarksDataTable fieldsDT)
		{
			if (fieldsDT == null || fieldsDT.Rows.Count <= 0)
				return false;
			IDataReader reader = null;


			if (erpDocument == null || erpDocument.Master == null)
				return false;

			try
			{
				if (tbCommand == null)
				{
					tbConnection = new TBConnection(CUtility.OpenConnectionToCurrentCompany());
					tbCommand = PrepareCommand(fieldsDT);
				}
				else
					SetParametersValue(fieldsDT);

				reader = tbCommand.ExecuteReader();
				while (reader.Read())
				{
					foreach (MSqlRecordItem recItem in MasterRecord.GetFieldsNoExtensions())
					{
						if (recItem.IsSegmentKey && !(recItem is MLocalSqlRecordItem))
							recItem.Value = reader[recItem.Name];
					}

					erpDocument.BrowseRecord();
					break;
				}
				reader.Close();
				return erpDocument.ValidCurrentRecord();
			}
			catch (TBException err)
			{
				SetMessage("An error occurred finding ERP document", err, "FindDocument");
				if (reader != null)
					reader.Close();
				tbCommand.Dispose();
				tbCommand = null;
				return false;
			}
		}


		//-----------------------------------------------------------------------------------------------
		public bool FindDocument(string primaryKey)
		{
			if (string.IsNullOrEmpty(primaryKey))
				return false;

			if (string.Compare(oldPrimaryKey, primaryKey, true) == 0)
				return true;
			
			erpDocument.BrowseRecord(primaryKey);
			oldPrimaryKey = primaryKey;
			return erpDocument.ValidCurrentRecord();
		}

		//-----------------------------------------------------------------------------------------------	
		public void CloseDocument()
		{
			if (erpDocument != null)
			{
				erpDocument.Close();
				erpDocument = null;
			}

			if (tbCommand != null)
				tbCommand.Dispose();

			if (tbConnection != null && tbConnection.State != ConnectionState.Closed)
			{
				tbConnection.Close();
				tbConnection.Dispose();
				tbConnection = null;
			}
		}


		//---------------------------------------------------------------------
		public bool Attach(int archivedDocId, string primaryKey, ref int attachmentId)
		{
            if (FindDocument(primaryKey))
            {
                string result = string.Empty;
                if (CUtility.AttachArchivedDocument(archivedDocId, DocumentHandle, ref attachmentId, result)) 
					return true;
				
				documentDiagnostic.Set(DiagnosticType.Error, string.Format(Strings.ErrorAttachingFile, archivedDocId), result.ToString());
				foreach (string mess in erpDocument.GetAllMessages())
					documentDiagnostic.Set(DiagnosticType.Error, mess);
            }
			else
				documentDiagnostic.Set(DiagnosticType.Error, string.Format(Strings.ERPDocumentNotExists, CUtility.GetDocumentTitle(docNamespace), primaryKey));
				
            return false;
		}

        //TODO MESSAGGI DA QUI?
        //-----------------------------------------------------------------------------------------------
        public string GetMessages()
        {

            if (erpDocument == null || erpDocument.Master == null)
                return null;

            return null;
        }
		
	}
}
