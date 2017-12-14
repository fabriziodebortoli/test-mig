using System;
using System.IO;

using Microarea.EasyAttachment.Components;
using Microarea.EasyAttachment.Core;
using Microarea.TBPicComponents;

namespace Microarea.EasyAttachment.BusinessLogic
{
	//================================================================================
	public class OCRManager : BaseManager
	{
		private DMSModelDataContext dc = null;
	
		private CoreOCRManager coreOCRMng = null;

		//-------------------------------------------------------------------------------
		public int ImageId { get { return coreOCRMng.ImageId; } }
		public int PdfPageCount { get { return coreOCRMng.PdfPageCount; } }
		public int PdfCurrentPage { get { return coreOCRMng.PdfCurrentPage; } }
		public bool IsPdfFile { get { return coreOCRMng.IsPdfFile; } }

		//-------------------------------------------------------------------------------
		public OCRManager(DMSOrchestrator dmsOrch)
		{
            DMSOrchestrator = dmsOrch;
            dc = dmsOrch.DataContext;

            coreOCRMng = new CoreOCRManager(dc, DMSOrchestrator.EasyAttachmentTempPath, DMSOrchestrator.OCRDictionary);
        }

        //---------------------------------------------------------------------
        public bool PdfGotoPage(int gotoPage)
		{
			return coreOCRMng.PdfRenderPage(gotoPage);
		}

		//---------------------------------------------------------------------
		public void PdfCloseDocument()
		{
			coreOCRMng.PdfCloseDocument();
		}	

		//---------------------------------------------------------------------
		public bool PdfSelectNextPage()
		{
			return coreOCRMng.PdfSelectNextPage();
		}

		//---------------------------------------------------------------------
		public bool PdfSelectPreviousPage()
		{
			return coreOCRMng.PdfSelectPreviousPage();
		}

		//---------------------------------------------------------------------
		public string GetOCRTextArea(int page, int leftArea, int topArea, int widthArea, int heightArea)
		{
			return coreOCRMng.GetOCRTextArea(leftArea, topArea, widthArea, heightArea);
		}

		//---------------------------------------------------------------------
		public string GetPdfOCRTextArea(int page, float leftArea, float topArea, float widthArea, float heightArea)
		{
			return coreOCRMng.GetPdfOCRTextArea(page, leftArea, topArea, widthArea, heightArea);
		}
		
		//---------------------------------------------------------------------
		internal bool LoadFromAttachment(AttachmentInfo currentAttach)
		{
            if (currentAttach == null || currentAttach.TempPath == null)
                return false;
			
			string ext = Path.GetExtension(currentAttach.TempPath);

			if (!CoreUtils.IsOCRCompatible(DMSOrchestrator.TextExtensions, ext))
				return false;

			try
			{
				switch (ext.ToLowerInvariant())
				{
					case FileExtensions.DotPdf:
						{
                            if (currentAttach.SaveAttachmentFile())
                                coreOCRMng.LoadFromFile(currentAttach.TempPath);
							break;
						}
					case FileExtensions.TxtCompatible:
						{
                            string pdfFileName = currentAttach.TransformToPdfA();
                            if (File.Exists(pdfFileName))
								coreOCRMng.LoadFromFile(pdfFileName);
							break;
						}
					default:
						{
							byte[] tempContent = null;

							if (!currentAttach.IsAFile && currentAttach.DocContent != null && currentAttach.DocContent.Length > 0)
							{
								try
								{
									tempContent = currentAttach.DocContent;
									coreOCRMng.LoadFromByteArray(tempContent);
								}
								catch (OutOfMemoryException)
								{
                                    // se il GdViewer non riesce a caricare i byte
                                    // provo a caricare partendo dal file temporaneo
                                    if (currentAttach.SaveAttachmentFile())
                                        coreOCRMng.LoadFromFile(currentAttach.TempPath);
								}
							}
							else
								if (currentAttach.VeryLargeFile || currentAttach.IsAFile)
									coreOCRMng.LoadFromFile(currentAttach.TempPath);

							if (
								(coreOCRMng.ImageId == 0 || coreOCRMng.Stat != TBPictureStatus.OK) &&
								 CoreUtils.IsTextCompatible(DMSOrchestrator.TextExtensions, ext)
								)
							{
								// faccio cmq la dispose
								tempContent = null;
								currentAttach.DisposeDocContent();

								goto case FileExtensions.TxtCompatible;
							}

							tempContent = null;
							currentAttach.DisposeDocContent();

							break;
						}
				}
			}
			catch (Exception e)
			{
				SetMessage(string.Format(Strings.ErrorGettingImageId, currentAttach.TempPath), e, "LoadFromAttachment");
			}

			return coreOCRMng.Stat == TBPictureStatus.OK;
		}
	}
}
