using System;

using Microarea.EasyAttachment.Components;
using Microarea.EasyAttachment.Core;
using Microarea.EasyAttachment.UI.Forms;

namespace Microarea.EasyAttachment.BusinessLogic
{
	///<summary>
	/// Gestore per la duplicazione dei documenti
	///</summary>
	//================================================================================
	public class DuplicateManager : BaseManager
	{
		//--------------------------------------------------------------------------------
		public DuplicateManager(DMSOrchestrator dmsOrch)
		{
			ManagerName = "DuplicateManager";
			DMSOrchestrator = dmsOrch;
		}

		/// <summary>
		/// Because DateTime Equals method uses Tick to do the comparison, it introduced
		/// the granularity I do not want.  
		/// This tool method determine if 2 DateTime's difference is less than certain 
		/// granularity(for example, 1 second). 
		/// </summary>
		//---------------------------------------------------------------------
		private bool DateTimeEqual(DateTime dt1, DateTime dt2)
		{
			TimeSpan ts = dt1.Subtract(dt2);
			return ts.Ticks < TimeSpan.TicksPerSecond;
		}

		//---------------------------------------------------------------------
		public CheckDuplicateResult CheckDuplicate(DMS_ArchivedDocument oldArchivedDoc, DMS_ArchivedDocument newArchivedDoc)
		{
            //se sto partendo dalla directory temporanea (vedi report di woorm o file acquisiti da scanner) devo verificare che la directory sia differente
            //a meno che si tratti della vecchia directory temp di EasyAttachment 
            bool samePath =
                (
                ((bool)oldArchivedDoc.IsWoormReport && (bool)newArchivedDoc.IsWoormReport) ||      // BugFix #23218 archiviazione fincati da postazioni differenti                  
                (string.Compare(oldArchivedDoc.Path, DMSOrchestrator.OldEasyAttachmentTempPath) == 0 && string.Compare(newArchivedDoc.Path, DMSOrchestrator.EasyAttachmentTempPath) == 0) ||
                string.Compare(oldArchivedDoc.Path, newArchivedDoc.Path, StringComparison.CurrentCultureIgnoreCase) == 0
                );

            
            if (oldArchivedDoc.CRC != newArchivedDoc.CRC)
			{
				if (samePath)
					return (newArchivedDoc.LastWriteTimeUtc >= oldArchivedDoc.LastWriteTimeUtc) ? CheckDuplicateResult.MoreRecent : CheckDuplicateResult.LessRecent;
				return CheckDuplicateResult.DifferentFile;
			}
			
			//same CRC
			if (DateTimeEqual(newArchivedDoc.LastWriteTimeUtc, oldArchivedDoc.LastWriteTimeUtc))
				return (samePath) ? CheckDuplicateResult.SameFile : CheckDuplicateResult.DifferentPath;

			return CheckDuplicateResult.DifferentFile;
		}

		//---------------------------------------------------------------------
		private DuplicateDocumentAction GetExistingAction
			(
			DMS_ArchivedDocument oldArchivedDoc,
			DMS_ArchivedDocument newArchivedDoc,
			CheckDuplicateResult duplicateResult
			)
		{
			if (DMSOrchestrator.InUnattendedMode)
				return DMSOrchestrator.SettingsManager.UsersSettingState.Options.DuplicateOptionsState.ActionForBatch;
			else
			{
				switch (DMSOrchestrator.SettingsManager.UsersSettingState.Options.DuplicateOptionsState.ActionForDocument)
				{
					case DuplicateDocumentAction.AskMeBeforeAttachDoc:
						{
							DuplicateForm form = new DuplicateForm();
							return form.AskAction
								(
								new AttachmentInfo(oldArchivedDoc, DMSOrchestrator), 
								new AttachmentInfo(newArchivedDoc, DMSOrchestrator), 
								duplicateResult, 
								DMSOrchestrator.CurrentTabPage
								);
						}
					default:
						return DMSOrchestrator.SettingsManager.UsersSettingState.Options.DuplicateOptionsState.ActionForDocument;
				}
			}
		}

		//---------------------------------------------------------------------
		public DuplicateDocumentAction GetExistingActionForBarcode(DMS_ArchivedDocument oldArchivedDoc, DMS_ArchivedDocument newArchivedDoc)
		{
			// se sono in unattended ritorno subito il valore del parametro per le batch
			if (DMSOrchestrator.InUnattendedMode)
				return DMSOrchestrator.SettingsManager.UsersSettingState.Options.BarcodeDetectionOptionsState.BCActionForBatch;

			// se il valore del parametro per i documents e' Ask visualizzo la form di duplicazione
			if (
				DMSOrchestrator.SettingsManager.UsersSettingState.Options.BarcodeDetectionOptionsState.BCActionForDocument
				== DuplicateDocumentAction.AskMeBeforeAttachDoc
				)
			{
				DuplicateForm df = new DuplicateForm();
				return df.AskAction
					(
					new AttachmentInfo(oldArchivedDoc, DMSOrchestrator),
					new AttachmentInfo(newArchivedDoc, DMSOrchestrator),
					CheckDuplicateResult.SameBarcode,
					DMSOrchestrator.CurrentTabPage
					);
			}

			return DMSOrchestrator.SettingsManager.UsersSettingState.Options.BarcodeDetectionOptionsState.BCActionForDocument;
		}

		//---------------------------------------------------------------------
		public DuplicateDocumentAction CheckDuplicateArchivedDocument(DMS_ArchivedDocument oldArchivedDoc, 	DMS_ArchivedDocument newArchivedDoc)
		{
			if (
				DMSOrchestrator.SettingsManager.UsersSettingState.Options.BarcodeDetectionOptionsState.EnableBarcode &&
				oldArchivedDoc.Barcode == newArchivedDoc.Barcode &&
				!string.IsNullOrWhiteSpace(newArchivedDoc.Barcode)
				)
			{
				DuplicateDocumentAction action = GetExistingActionForBarcode(oldArchivedDoc, newArchivedDoc);

				// in caso di stesso barcode e archiviazione multipla devo pulire il valore del barcode sul nuovo ArchiveDoc
				// occhio che se esiste un altro file archiviato uguale come caratteristiche (crc, data piu' recente, etc.)
				// mi viene inserito un archivedoc uguale //quindi???
				if (action == DuplicateDocumentAction.ArchiveAndKeepBothDocs)
					newArchivedDoc.Barcode = string.Empty;

				return action;
			}
		
			CheckDuplicateResult duplicateResult = CheckDuplicate(oldArchivedDoc, newArchivedDoc);

			// non mostro la DuplicateForm perche' il file e' lo stesso e non devo duplicare niente!
			if (duplicateResult == CheckDuplicateResult.SameFile)
				return DuplicateDocumentAction.UseExistingDoc;

			// se il file è differente allora permetto l'archiviaizone altrimenti in tutti gli altri casi vado a controllare quali parametri ho impostato
			return (duplicateResult == CheckDuplicateResult.DifferentFile) ? DuplicateDocumentAction.ArchiveAndKeepBothDocs : GetExistingAction(oldArchivedDoc, newArchivedDoc, duplicateResult);
		}
	}
}
