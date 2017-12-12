using System;
using System.Collections;

using Microarea.TaskBuilderNet.Core.TbWoormViewerInterface;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.WebServicesWrapper
{
	/// <summary>
	/// E' la classe che fa da proxy per l'oggetto CWoormInfo di Task Builder
	/// </summary>
	//=========================================================================
	public class WoormInfo : IDisposable
	{
		private int handle = 0;
		private ArrayList reportNamespaces = new ArrayList();
		private TbWoormViewer tbWoormViewer = null;
		private Microarea.TaskBuilderNet.Core.TbWoormViewerInterface.TBHeaderInfo tbHeader = null;
		private bool disposed = false;
		
		//-----------------------------------------------------------------------
		public WoormInfo(TbWoormViewer tbWoormViewer, Microarea.TaskBuilderNet.Core.TbWoormViewerInterface.TBHeaderInfo tbHeader, string reportNamespace)
		{
			if (tbWoormViewer == null)
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.WoormInfoError));

			this.tbWoormViewer = tbWoormViewer;
			this.tbHeader = tbHeader;
			this.reportNamespaces.Add(reportNamespace);
			try
			{
				handle = tbWoormViewer.CWoormInfo_Create(new CWoormInfo_CreateIn(tbHeader, reportNamespace)).@return;
			}
			catch(Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.WoormInfoError, e));
			}
		}
		
		//---------------------------------------------------------------------
		~WoormInfo()
		{
			Dispose(false);
		}

		//---------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
		}
		
		// Dispose(bool disposing) executes in two distinct scenarios.
		// If disposing equals true, the method has been called directly
		// or indirectly by a user's code. Managed and unmanaged resources
		// can be disposed.
		// If disposing equals false, the method has been called by the 
		// runtime from inside the finalizer and you should not reference 
		// other objects. Only unmanaged resources can be disposed.
		private void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if (!this.disposed)
			{
				if (tbWoormViewer != null)
				{
					try
					{
						tbWoormViewer.CWoormInfo_Dispose(new CWoormInfo_DisposeIn(tbHeader, handle));
					}
					catch(Exception e)
					{
						throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.WoormInfoError, e));
					}
				}
				if (disposing)
					GC.SuppressFinalize(this);
			}
			disposed = true;         
		}

		//-----------------------------------------------------------------------
		public static implicit operator int(WoormInfo wi)
		{
			return wi.handle;
		}

		//-----------------------------------------------------------------------
		public override string ToString()
		{
			string returnString = string.Empty;
			foreach (string s in reportNamespaces)
			{
				if (returnString == string.Empty)
					returnString = s;
				else
					returnString += "\r\n" + s;
			}

			return returnString;
		}
	
		//-----------------------------------------------------------------------
		public void AddReport(string reportNamespace)
		{
			this.reportNamespaces.Add(reportNamespace);
			try
			{
				tbWoormViewer.CWoormInfo_AddReport(new CWoormInfo_AddReportIn(tbHeader, handle, reportNamespace));
			}
			catch(Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.WoormInfoError, e));
			}
		}

		//-----------------------------------------------------------------------
		public void SetAutoPrint(bool autoPrint)
		{
			try
			{
				tbWoormViewer.CWoormInfo_SetAutoPrint(new CWoormInfo_SetAutoPrintIn(tbHeader, handle, autoPrint));
			}
			catch(Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.WoormInfoError, e));
			}
		}

		//-----------------------------------------------------------------------
		public void SetCloseOnEndPrint(bool closeOnEndPrint)
		{
			try
			{
				tbWoormViewer.CWoormInfo_SetCloseOnEndPrint(new CWoormInfo_SetCloseOnEndPrintIn(tbHeader, handle, closeOnEndPrint));
			}
			catch(Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.WoormInfoError, e));
			}
		}

		//-----------------------------------------------------------------------
		public void SetIconized(bool iconized)
		{
			try
			{
				tbWoormViewer.CWoormInfo_SetIconized(new CWoormInfo_SetIconizedIn(tbHeader, handle, iconized));
			}
			catch(Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.WoormInfoError, e));
			}
		}

		//-----------------------------------------------------------------------
		public void SetHideFrame(bool hideFrame)
		{
			try
			{
				tbWoormViewer.CWoormInfo_SetHideFrame(new CWoormInfo_SetHideFrameIn(tbHeader, handle, hideFrame));
			}
			catch(Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.WoormInfoError, e));
			}
		}

        //-----------------------------------------------------------------------
        public void SetSilentMode(bool silent)
        {
            try
            {
                tbWoormViewer.CWoormInfo_SetSilentMode(new CWoormInfo_SetSilentModeIn(tbHeader, handle, silent));
            }
            catch (Exception e)
            {
                throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.WoormInfoError, e));
            }
        }

		//-----------------------------------------------------------------------
		public void SetSendEmail(bool sendEmail)
		{
			try
			{
				tbWoormViewer.CWoormInfo_SetSendEmail(new CWoormInfo_SetSendEmailIn(tbHeader, handle, sendEmail));
			}
			catch(Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.WoormInfoError, e));
			}
		}

		//-----------------------------------------------------------------------
		public void SetAttachRDE(bool attachRDE)
		{
			try
			{
				tbWoormViewer.CWoormInfo_SetAttachRDE(new CWoormInfo_SetAttachRDEIn(tbHeader, handle, attachRDE));
			}
			catch(Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.WoormInfoError, e));
			}
		}

		//-----------------------------------------------------------------------
		public void SetAttachPDF(bool attachPDF)
		{
			try
			{
				tbWoormViewer.CWoormInfo_SetAttachPDF(new CWoormInfo_SetAttachPDFIn(tbHeader, handle, attachPDF));
			}
			catch(Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.WoormInfoError, e));
			}
		}

		//-----------------------------------------------------------------------
		public void SetCompressAttach(bool compressAttach)
		{
			try
			{
				tbWoormViewer.CWoormInfo_SetCompressAttach(new CWoormInfo_SetCompressAttachIn(tbHeader, handle, compressAttach));
			}
			catch(Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.WoormInfoError, e));
			}
		}

		//-----------------------------------------------------------------------
		public void SetMailTo(string mailRecipients)
		{
			if (mailRecipients == null || mailRecipients == String.Empty)
				return;

			try
			{
				tbWoormViewer.CWoormInfo_MailTo(new CWoormInfo_MailToIn(tbHeader, handle, mailRecipients));
			}
			catch(Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.WoormInfoError, e));
			}
		}

        //-----------------------------------------------------------------------
        public void SetMailSubject(string subject)
        {
            if (subject == null || subject == String.Empty)
                return;

            try
            {
                tbWoormViewer.CWoormInfo_MailSubject(new CWoormInfo_MailSubjectIn(tbHeader, handle, subject));
            }
            catch (Exception e)
            {
                throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.WoormInfoError, e));
            }
        }

        //-----------------------------------------------------------------------
        public void SetMailBody(string body, bool isHtml)
        {
            if (body == null || body == String.Empty)
                return;

            try
            {
                tbWoormViewer.CWoormInfo_MailBody(new CWoormInfo_MailBodyIn(tbHeader, handle, body, isHtml));
            }
            catch (Exception e)
            {
                throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.WoormInfoError, e));
            }
        }

		//-----------------------------------------------------------------------
		public void SetOnePrintDialog(bool onePrintDialog)
		{
			try
			{
				tbWoormViewer.CWoormInfo_SetOnePrintDialog(new CWoormInfo_SetOnePrintDialogIn(tbHeader, handle, onePrintDialog));
			}
			catch(Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.WoormInfoError, e));
			}
		}

		//-----------------------------------------------------------------------
		public void SetUniqueMail(bool uniqueMail)
		{
			try
			{
				tbWoormViewer.CWoormInfo_SetUniqueMail(new CWoormInfo_SetUniqueMailIn(tbHeader, handle, uniqueMail));
			}
			catch(Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.WoormInfoError, e));
			}
		}

		//-----------------------------------------------------------------------
		public void SetConcatPDF(bool concatPDF)
		{
			try
			{
				tbWoormViewer.CWoormInfo_SetConcatPDF(new CWoormInfo_SetConcatPDFIn(tbHeader, handle, concatPDF));
			}
			catch(Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.WoormInfoError, e));
			}
		}

		//-----------------------------------------------------------------------
		public void SetPDFOutput(bool PDFOutput)
		{
			try
			{
				tbWoormViewer.CWoormInfo_SetPDFOutput(new CWoormInfo_SetPDFOutputIn(tbHeader, handle, PDFOutput));
			}
			catch(Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.WoormInfoError, e));
			}
		}

		//-----------------------------------------------------------------------
		public void SetPDFOutputPreview(bool PDFOutputPreview)
		{
			try
			{
				tbWoormViewer.CWoormInfo_SetPDFOutputPreview(new CWoormInfo_SetPDFOutputPreviewIn(tbHeader, handle, PDFOutputPreview));
			}
			catch(Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.WoormInfoError, e));
			}
		}

		//-----------------------------------------------------------------------
		public void SetRDEOutput(bool RDEOutput)
		{
			try
			{
				tbWoormViewer.CWoormInfo_SetRDEOutput(new CWoormInfo_SetRDEOutputIn(tbHeader, handle, RDEOutput));
			}
			catch(Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.WoormInfoError, e));
			}
		}

		//-----------------------------------------------------------------------
		public void SetExcelOutput(bool ExcelOutput)
		{
			try
			{
				tbWoormViewer.CWoormInfo_SetExcelOutput(new CWoormInfo_SetExcelOutputIn(tbHeader, handle, ExcelOutput));
			}
			catch(Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.WoormInfoError, e));
			}
		}

		//-----------------------------------------------------------------------
		public void AddOutputFileName(string fileName)
		{
			try
			{
				tbWoormViewer.CWoormInfo_AddOutputFileName(new CWoormInfo_AddOutputFileNameIn(tbHeader, handle, fileName));
			}
			catch(Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.WoormInfoError, e));
			}
		}

        //-----------------------------------------------------------------------
        public void SetPrinterName(string printerName)
        {
            try
            {
                tbWoormViewer.CWoormInfo_SetPrinterName(new CWoormInfo_SetPrinterNameIn(tbHeader, handle, printerName));
            }
            catch (Exception e)
            {
                throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.WoormInfoError, e));
            }
        }

        //-----------------------------------------------------------------------
        public void SetExportOutputType(string expType)
        {
            try
            {
                tbWoormViewer.CWoormInfo_SetExportOutputType(new CWoormInfo_SetExportOutputTypeIn(tbHeader, handle, expType));
            }
            catch (Exception e)
            {
                throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.WoormInfoError, e));
            }
        }
 	}
}
