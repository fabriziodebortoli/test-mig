using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Interfaces;
using System.Diagnostics;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.WebServices.TbServices
{
	//=========================================================================
	public class TbDMSEngine
	{
		private const int timeout = 15000;
		internal Diagnostic diagnostic = new Diagnostic("TbDMS");  	//Gestione errori
		string locker = "No locking method";
			
		//---------------------------------------------------------------------------
		internal bool TryLockResources()
		{
			try
			{
				if (!Monitor.TryEnter(this, timeout))
				{
					diagnostic.Set
						(DiagnosticType.LogInfo | DiagnosticType.Warning,
						string.Format("Failed to lock LoginManager after {0} milliseconds. Method: {1}", timeout, locker));
					return false;
				}

			}
			catch (Exception exc)
			{
				diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, "TryLockResources: " + exc.ToString());
				throw exc;
			} try
			{

				locker = (new StackTrace()).GetFrame(1).GetMethod().Name;
			}
			catch { locker = "Unknown method"; }
			return true;
		}

		//---------------------------------------------------------------------------
		internal void ReleaseResources()
		{
			try
			{
				Monitor.Exit(this);
				locker = "No locking method";
			}
			catch (Exception exc)
			{
				diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, "ReleaseResources: " + exc.ToString());
				throw exc;
			}
		}

		//---------------------------------------------------------------------------
		private TbLoaderClientInterface InitializeTbLoader(string authenticationToken)
		{
			string user = string.Empty;

			int tbPort = TbServicesApplication.TbServicesEngine.CreateTB(authenticationToken, DateTime.Now, true, out user);
			if (tbPort < 0)
				throw new Exception(string.Format(Strings.TbLoaderIstancingError, tbPort.ToString()));

			return new TbLoaderClientInterface(BasePathFinder.BasePathFinderInstance, tbPort, authenticationToken, TbLoaderInfo.TbWCFBinding);

		}

		//---------------------------------------------------------------------------
		internal bool ArchiveFile(string authenticationToken, string fileName, string description, ref string result)
		{
			using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
				return tb.ArchiveFile(fileName, description, ref result);
		}

		//-----------------------------------------------------------------------
		public bool ArchiveFolder(string authenticationToken, string folder, ref string result)
		{
			using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
				return tb.ArchiveFolder(folder, ref result);
		}
		
		//-----------------------------------------------------------------------
		public bool AttachArchivedDocument(string authenticationToken, int archivedDocId, int documentHandle, ref string result)
		{
			using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
				return tb.AttachArchivedDocument(archivedDocId, documentHandle, ref result);
		}

		//-----------------------------------------------------------------------
		public bool AttachFile(string authenticationToken, string fileName, string description, int documentHandle, ref string result)
		{
			using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
				return tb.AttachFile(fileName, description, documentHandle, ref result);
		}
		
		//-----------------------------------------------------------------------
		public bool AttachFileInDocument(string authenticationToken, string documentNamespace, string documentKey, string fileName, string description, ref string result)
		{
			using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
				return tb.AttachFileInDocument(documentNamespace, documentKey, fileName, description, ref result);
		}
		
		//-----------------------------------------------------------------------
		public bool AttachFolder(string authenticationToken, string folder, int documentHandle, ref string result)
		{
			using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
				return tb.AttachFolder(folder, documentHandle, ref result);
		}
		
		//-----------------------------------------------------------------------
		public bool AttachFolderInDocument(string authenticationToken, string documentNamespace, string documentKey, string folder, ref string result)
		{
			using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
				return tb.AttachFolderInDocument(documentNamespace, documentKey, folder, ref result);
		}
		
		//-----------------------------------------------------------------------
		public bool AttachFromTable(string authenticationToken, string documentNamespace, string documentKey, ref string result)
		{
			using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
				return tb.AttachFromTable(documentNamespace, documentKey, ref result);
		}

		//-----------------------------------------------------------------------
		public bool AttachPaperyBarcode(string authenticationToken, int documentHandle, string barcode)
		{
			using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
				return tb.AttachPaperyBarcode(documentHandle, barcode);
		}

		//-----------------------------------------------------------------------
		public bool AttachPaperyInDocument(string authenticationToken, string documentNamespace, string documentKey, string barcode, string description, ref string result)
		{
			using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
				return tb.AttachPaperyInDocument(documentNamespace, documentKey, barcode, description, ref result);
		}
		
		//-----------------------------------------------------------------------
		public string GetAttachmentTemporaryFilePath(string authenticationToken, int attachmentID)
		{
			using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
				return tb.GetAttachmentTemporaryFilePath(attachmentID);
		}

		//-----------------------------------------------------------------------
		public bool GetDefaultBarcodeType(string authenticationToken, ref string type, ref string prefix)
		{
			using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
				return tb.GetDefaultBarcodeType(ref type, ref prefix);
		}
		
		//-----------------------------------------------------------------------
		public string GetEasyAttachmentTempPath(string authenticationToken)
		{
			using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
				return tb.GetEasyAttachmentTempPath();
		}

		//-----------------------------------------------------------------------
		public string GetNewBarcodeValue(string authenticationToken)
		{
			using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
				return tb.GetNewBarcodeValue();
		}
		
		//-----------------------------------------------------------------------
		public bool MassiveAttachUnattendedMode(string authenticationToken, string folder, bool splitFile, ref string result)
		{
			using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
				return tb.MassiveAttachUnattendedMode(folder, splitFile, ref result);
		}
		
		//-----------------------------------------------------------------------
		public string SaveAttachmentFileInFolder(string authenticationToken, int attachmentID, string sharedFolder)
		{
			using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
				return tb.SaveAttachmentFileInFolder(attachmentID, sharedFolder);
		}
		
		//-----------------------------------------------------------------------
		public int[] SearchAttachmentsForDocument(string authenticationToken, string documentNamespace, string documentKey, string searchText, int location, string searchFields)
		{
			using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
				return tb.SearchAttachmentsForDocument(documentNamespace, documentKey, searchText, location, searchFields);
		}

        //-----------------------------------------------------------------------
        public bool GetAttachmentBinaryContent(string authenticationToken, int attachmentID, ref byte[] binaryContent, ref string fileName, ref bool veryLargeFile)
        {
            using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
                return tb.GetAttachmentBinaryContent(attachmentID, ref binaryContent, ref fileName, ref veryLargeFile);
        }

        //-----------------------------------------------------------------------
        public bool ArchiveBinaryContent(string authenticationToken, byte[] binaryContent, string sourceFileName, string description, ref int archiveDocID, ref string result)
        {
            using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
                return tb.ArchiveBinaryContent(binaryContent, sourceFileName, description, ref archiveDocID, ref result);
        }

        //-----------------------------------------------------------------------
        public bool AttachBinaryContent(string authenticationToken, byte[] binaryContent, string sourceFileName, string description, int documentHandle, ref int attachmentID, ref string result)
        {
            using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
                return tb.AttachBinaryContent(binaryContent, sourceFileName, description, documentHandle, ref attachmentID, ref result);
        }

        //-----------------------------------------------------------------------
        public bool AttachBinaryContentInDocument(string authenticationToken, byte[] binaryContent, string sourceFileName, string description, string documentNamespace, string documentKey, ref int attachmentID, ref string result)
        {
            using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
                return tb.AttachBinaryContentInDocument(binaryContent, sourceFileName, description, documentNamespace, documentKey, ref attachmentID, ref result);
        }

        //-----------------------------------------------------------------------
        public int GetAttachmentIDByFileName(string authenticationToken, string documentNamespace, string documentKey, string fileName)
        {
            using (TbLoaderClientInterface tb = InitializeTbLoader(authenticationToken))
                return tb.GetAttachmentIDByFileName(documentNamespace, documentKey, fileName);
        }
    }

	/// <summary>
	/// Callse che contiene l'istanza statica dell'applicazione
	/// </summary>
	//=========================================================================
	public class TbDMSApplication
	{
		public static TbDMSEngine TbDMSEngine = new TbDMSEngine();
	}
}