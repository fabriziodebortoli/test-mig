using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using System.Reflection;

namespace Microarea.WebServices.TbServices
{
	//==================================================================================
	[WebService(Namespace="http://microarea.it/TbServices/")]
	[System.ComponentModel.ToolboxItem(false)]

	public class TbDMS : System.Web.Services.WebService
	{
        [WebMethod]
		//---------------------------------------------------------------------------
		public bool ArchiveFile(string authenticationToken, string fileName, string description, ref string result)
		{
			if (!TbDMSApplication.TbDMSEngine.TryLockResources())
				throw new Exception(Strings.ResourcesTimeout);

			try
			{
				return TbDMSApplication.TbDMSEngine.ArchiveFile(authenticationToken, fileName, description, ref result);
			}
			catch (Exception exc)
			{
				TbDMSApplication.TbDMSEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
				throw;
			}
			finally
			{
				TbDMSApplication.TbDMSEngine.ReleaseResources();
			}
        }


		[WebMethod]
		//-----------------------------------------------------------------------
		public bool ArchiveFolder(string authenticationToken, string folder, ref string result)
		{
			if (!TbDMSApplication.TbDMSEngine.TryLockResources())
				throw new Exception(Strings.ResourcesTimeout);

			try
			{
				return TbDMSApplication.TbDMSEngine.ArchiveFolder(authenticationToken, folder, ref result);
			}
			catch (Exception exc)
			{
				TbDMSApplication.TbDMSEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
				throw;
			}
			finally
			{
				TbDMSApplication.TbDMSEngine.ReleaseResources();
			}
		}

        [WebMethod]
        //---------------------------------------------------------------------------
        public bool ArchiveBinaryContent(string authenticationToken, byte[] binaryContent, string sourceFileName, string description, ref int archiveDocID, ref string result)
        {
            if (!TbDMSApplication.TbDMSEngine.TryLockResources())
                throw new Exception(Strings.ResourcesTimeout);

            try
            {
                return TbDMSApplication.TbDMSEngine.ArchiveBinaryContent(authenticationToken, binaryContent, sourceFileName, description, ref archiveDocID, ref result);
            }
            catch (Exception exc)
            {
                TbDMSApplication.TbDMSEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                TbDMSApplication.TbDMSEngine.ReleaseResources();
            }
        }

        [WebMethod]
		//-----------------------------------------------------------------------
		public bool AttachArchivedDocument(string authenticationToken, int archivedDocId, int documentHandle, ref string result)
		{
			if (!TbDMSApplication.TbDMSEngine.TryLockResources())
				throw new Exception(Strings.ResourcesTimeout);

			try
			{
				return TbDMSApplication.TbDMSEngine.AttachArchivedDocument(authenticationToken, archivedDocId, documentHandle, ref result);
			}
			catch (Exception exc)
			{
				TbDMSApplication.TbDMSEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
				throw;
			}
			finally
			{
				TbDMSApplication.TbDMSEngine.ReleaseResources();
			}
		}

        [WebMethod]
        //---------------------------------------------------------------------------
        public bool AttachBinaryContent(string authenticationToken, byte[] binaryContent, string sourceFileName, string description, int documentHandle, ref int attachmentID, ref string result)
        {
            if (!TbDMSApplication.TbDMSEngine.TryLockResources())
                throw new Exception(Strings.ResourcesTimeout);

            try
            {
                return TbDMSApplication.TbDMSEngine.AttachBinaryContent(authenticationToken, binaryContent, sourceFileName, description, documentHandle, ref attachmentID, ref result);
            }
            catch (Exception exc)
            {
                TbDMSApplication.TbDMSEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                TbDMSApplication.TbDMSEngine.ReleaseResources();
            }
        }


        [WebMethod]
        //---------------------------------------------------------------------------
        public bool AttachBinaryContentInDocument(string authenticationToken, byte[] binaryContent, string sourceFileName, string description, string documentNamespace, string documentKey, ref int attachmentID, ref string result)
        {
            if (!TbDMSApplication.TbDMSEngine.TryLockResources())
                throw new Exception(Strings.ResourcesTimeout);

            try
            {
                return TbDMSApplication.TbDMSEngine.AttachBinaryContentInDocument(authenticationToken, binaryContent, sourceFileName, description, documentNamespace, documentKey, ref attachmentID, ref result);
            }
            catch (Exception exc)
            {
                TbDMSApplication.TbDMSEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                TbDMSApplication.TbDMSEngine.ReleaseResources();
            }
        }

        [WebMethod]
		//-----------------------------------------------------------------------
		public bool AttachFile(string authenticationToken, string fileName, string description, int documentHandle, ref string result)
		{
			if (!TbDMSApplication.TbDMSEngine.TryLockResources())
				throw new Exception(Strings.ResourcesTimeout);

			try
			{
				return TbDMSApplication.TbDMSEngine.AttachFile(authenticationToken, fileName, description, documentHandle, ref result);
			}
			catch (Exception exc)
			{
				TbDMSApplication.TbDMSEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
				throw;
			}
			finally
			{
				TbDMSApplication.TbDMSEngine.ReleaseResources();
			}
		}

		[WebMethod]
		//-----------------------------------------------------------------------
		public bool AttachFileInDocument(string authenticationToken, string documentNamespace, string documentKey, string fileName, string description, ref string result)
		{
			if (!TbDMSApplication.TbDMSEngine.TryLockResources())
				throw new Exception(Strings.ResourcesTimeout);

			try
			{
				return TbDMSApplication.TbDMSEngine.AttachFileInDocument(authenticationToken, documentNamespace, documentKey, fileName, description, ref result);
			}
			catch (Exception exc)
			{
				TbDMSApplication.TbDMSEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
				throw;
			}
			finally
			{
				TbDMSApplication.TbDMSEngine.ReleaseResources();
			}
		}

		[WebMethod]
		//-----------------------------------------------------------------------
		public bool AttachFolder(string authenticationToken, string folder, int documentHandle, ref string result)
		{
			if (!TbDMSApplication.TbDMSEngine.TryLockResources())
				throw new Exception(Strings.ResourcesTimeout);

			try
			{
				return TbDMSApplication.TbDMSEngine.AttachFolder(authenticationToken, folder, documentHandle, ref result);
			}
			catch (Exception exc)
			{
				TbDMSApplication.TbDMSEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
				throw;
			}
			finally
			{
				TbDMSApplication.TbDMSEngine.ReleaseResources();
			}
		}

		[WebMethod]
		//-----------------------------------------------------------------------
		public bool AttachFolderInDocument(string authenticationToken, string documentNamespace, string documentKey, string folder, ref string result)
		{
			if (!TbDMSApplication.TbDMSEngine.TryLockResources())
				throw new Exception(Strings.ResourcesTimeout);

			try
			{
				return TbDMSApplication.TbDMSEngine.AttachFolderInDocument(authenticationToken, documentNamespace, documentKey, folder, ref result);
			}
			catch (Exception exc)
			{
				TbDMSApplication.TbDMSEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
				throw;
			}
			finally
			{
				TbDMSApplication.TbDMSEngine.ReleaseResources();
			}
		}

		[WebMethod]
		//-----------------------------------------------------------------------
		public bool AttachFromTable(string authenticationToken, string documentNamespace, string documentKey, ref string result)
		{
			if (!TbDMSApplication.TbDMSEngine.TryLockResources())
				throw new Exception(Strings.ResourcesTimeout);

			try
			{
				return TbDMSApplication.TbDMSEngine.AttachFromTable(authenticationToken, documentNamespace, documentKey, ref result);
			}
			catch (Exception exc)
			{
				TbDMSApplication.TbDMSEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
				throw;
			}
			finally
			{
				TbDMSApplication.TbDMSEngine.ReleaseResources();
			}

		}

		[WebMethod]
		//-----------------------------------------------------------------------
		public bool AttachPaperyBarcode(string authenticationToken, int documentHandle, string barcode)
		{
			if (!TbDMSApplication.TbDMSEngine.TryLockResources())
				throw new Exception(Strings.ResourcesTimeout);

			try
			{
				return TbDMSApplication.TbDMSEngine.AttachPaperyBarcode(authenticationToken, documentHandle, barcode);
			}
			catch (Exception exc)
			{
				TbDMSApplication.TbDMSEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
				throw;
			}
			finally
			{
				TbDMSApplication.TbDMSEngine.ReleaseResources();
			}
		}

		[WebMethod]
		//-----------------------------------------------------------------------
		public bool AttachPaperyInDocument(string authenticationToken, string documentNamespace, string documentKey, string barcode, string description, ref string result)
		{
			if (!TbDMSApplication.TbDMSEngine.TryLockResources())
				throw new Exception(Strings.ResourcesTimeout);

			try
			{
				return TbDMSApplication.TbDMSEngine.AttachPaperyInDocument(authenticationToken, documentNamespace, documentKey, barcode, description, ref result);
			}
			catch (Exception exc)
			{
				TbDMSApplication.TbDMSEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
				throw;
			}
			finally
			{
				TbDMSApplication.TbDMSEngine.ReleaseResources();
			}
		}

		[WebMethod]
		//-----------------------------------------------------------------------
		public string GetAttachmentTemporaryFilePath(string authenticationToken, int attachmentID)
		{
			if (!TbDMSApplication.TbDMSEngine.TryLockResources())
				throw new Exception(Strings.ResourcesTimeout);

			try
			{
				return TbDMSApplication.TbDMSEngine.GetAttachmentTemporaryFilePath(authenticationToken, attachmentID);
			}
			catch (Exception exc)
			{
				TbDMSApplication.TbDMSEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
				throw;
			}
			finally
			{
				TbDMSApplication.TbDMSEngine.ReleaseResources();
			}
		}

        [WebMethod]
        //---------------------------------------------------------------------------
        public int GetAttachmentIDByFileName(string authenticationToken, string documentNamespace, string documentKey, string fileName)
        {
            if (!TbDMSApplication.TbDMSEngine.TryLockResources())
                throw new Exception(Strings.ResourcesTimeout);

            try
            {
                return TbDMSApplication.TbDMSEngine.GetAttachmentIDByFileName(authenticationToken, documentNamespace, documentKey, fileName);
            }
            catch (Exception exc)
            {
                TbDMSApplication.TbDMSEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                TbDMSApplication.TbDMSEngine.ReleaseResources();
            }
        }
        [WebMethod]
		//-----------------------------------------------------------------------
		public bool GetDefaultBarcodeType(string authenticationToken, ref string type, ref string prefix)
		{
			if (!TbDMSApplication.TbDMSEngine.TryLockResources())
				throw new Exception(Strings.ResourcesTimeout);

			try
			{
				return TbDMSApplication.TbDMSEngine.GetDefaultBarcodeType(authenticationToken, ref type, ref prefix);
			}
			catch (Exception exc)
			{
				TbDMSApplication.TbDMSEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
				throw;
			}
			finally
			{
				TbDMSApplication.TbDMSEngine.ReleaseResources();
			}
		}

		[WebMethod]
		//-----------------------------------------------------------------------
		public string GetEasyAttachmentTempPath(string authenticationToken)
		{
			if (!TbDMSApplication.TbDMSEngine.TryLockResources())
				throw new Exception(Strings.ResourcesTimeout);

			try
			{
				return TbDMSApplication.TbDMSEngine.GetEasyAttachmentTempPath(authenticationToken);
			}
			catch (Exception exc)
			{
				TbDMSApplication.TbDMSEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
				throw;
			}
			finally
			{
				TbDMSApplication.TbDMSEngine.ReleaseResources();
			}
		}

		[WebMethod]
		//-----------------------------------------------------------------------
		public string GetNewBarcodeValue(string authenticationToken)
		{
			if (!TbDMSApplication.TbDMSEngine.TryLockResources())
				throw new Exception(Strings.ResourcesTimeout);

			try
			{
				return TbDMSApplication.TbDMSEngine.GetNewBarcodeValue(authenticationToken);
			}
			catch (Exception exc)
			{
				TbDMSApplication.TbDMSEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
				throw;
			}
			finally
			{
				TbDMSApplication.TbDMSEngine.ReleaseResources();
			}
		}

		[WebMethod]
		//-----------------------------------------------------------------------
		public bool MassiveAttachUnattendedMode(string authenticationToken, string folder, bool splitFile, ref string result)
		{
			if (!TbDMSApplication.TbDMSEngine.TryLockResources())
				throw new Exception(Strings.ResourcesTimeout);

			try
			{
				return TbDMSApplication.TbDMSEngine.MassiveAttachUnattendedMode(authenticationToken, folder, splitFile, ref result);
			}
			catch (Exception exc)
			{
				TbDMSApplication.TbDMSEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
				throw;
			}
			finally
			{
				TbDMSApplication.TbDMSEngine.ReleaseResources();
			}
		}

		[WebMethod]
		//-----------------------------------------------------------------------
		public string SaveAttachmentFileInFolder(string authenticationToken, int attachmentID, string sharedFolder)
		{
			if (!TbDMSApplication.TbDMSEngine.TryLockResources())
				throw new Exception(Strings.ResourcesTimeout);

			try
			{
				return TbDMSApplication.TbDMSEngine.SaveAttachmentFileInFolder(authenticationToken, attachmentID, sharedFolder);
			}
			catch (Exception exc)
			{
				TbDMSApplication.TbDMSEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
				throw;
			}
			finally
			{
				TbDMSApplication.TbDMSEngine.ReleaseResources();
			}
		}

		[WebMethod]
		//-----------------------------------------------------------------------
		public int[] SearchAttachmentsForDocument(string authenticationToken, string documentNamespace, string documentKey, string searchText, int location, string searchFields)
		{
			if (!TbDMSApplication.TbDMSEngine.TryLockResources())
				throw new Exception(Strings.ResourcesTimeout);

			try
			{
				return TbDMSApplication.TbDMSEngine.SearchAttachmentsForDocument(authenticationToken, documentNamespace, documentKey, searchText, location, searchFields);
			}
			catch (Exception exc)
			{
				TbDMSApplication.TbDMSEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
				throw;
			}
			finally
			{
				TbDMSApplication.TbDMSEngine.ReleaseResources();
			}
		}


        [WebMethod]
        //---------------------------------------------------------------------------
        public bool GetAttachmentBinaryContent(string authenticationToken, int attachmentID, ref byte[] binaryContent, ref string fileName, ref bool bVeryLargeFile)
        {
            if (!TbDMSApplication.TbDMSEngine.TryLockResources())
                throw new Exception(Strings.ResourcesTimeout);

            try
            {
                return TbDMSApplication.TbDMSEngine.GetAttachmentBinaryContent(authenticationToken, attachmentID, ref binaryContent, ref fileName, ref bVeryLargeFile);
            }
            catch (Exception exc)
            {
                TbDMSApplication.TbDMSEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                TbDMSApplication.TbDMSEngine.ReleaseResources();
            }
        }

    }
}
