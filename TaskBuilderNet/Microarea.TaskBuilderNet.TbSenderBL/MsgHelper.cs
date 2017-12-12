using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microarea.TaskBuilderNet.TbSenderBL.Pdf;
using Microarea.TaskBuilderNet.TbSenderBL.PostaLite;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using Microarea.TaskBuilderNet.TbSenderBL.postalite.api;

namespace Microarea.TaskBuilderNet.TbSenderBL
{
	//===================================================================================
	public static class MsgHelper
	{
		//-------------------------------------------------------------------------------
		public static string GetSenderMultiLine(PostaliteSettings plSettings)
		{
			if (plSettings == null)
				return string.Empty;
			string cmpName = plSettings.AddresserCompanyName;
			string address = plSettings.AddresserAddress;
			string zip = plSettings.AddresserZipCode;
			string city = plSettings.AddresserCity;
			string county = plSettings.AddresserCounty;
			//string country = plSettings.AddresserCountry;
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(cmpName);
			sb.AppendLine(address);
			sb.AppendFormat(CultureInfo.InvariantCulture, "{0} {1} ({2})", zip, city, county);
			return sb.ToString();
		}

		//-------------------------------------------------------------------------------
		public static IFileMessage GetFileMessage(TB_MsgLots lot, string company, IPostaLiteSettingsProvider postaLiteSettingsProvider)
		{
			StringBuilder sbPdfFileName = new StringBuilder();
			sbPdfFileName.AppendFormat(CultureInfo.InvariantCulture, "{0}_lot_ID_{1}.pdf", company, lot.LotID);
			// assicura che company non abbia caratteri strani, pulendola
			sbPdfFileName.Replace('/', '_'); // TODO controlla altri caratteri strani
			string pdfFileName = sbPdfFileName.ToString();

			PostaliteSettings plSettings = postaLiteSettingsProvider.GetSettings(company);
			string senderMultiLine = GetSenderMultiLine(plSettings); // legge anagrafica mittente da MA_Company
			// es:
			// Microarea S.p.A.
			// Via Renata Bianchi, 36
			// 16152 Genova (GE)

			byte[] bytes;
			//string contentBase64;
			using (PdfDocument pdf = BuildCombinedPdf(lot, senderMultiLine, company))
			{
				//pdf.Save(mainPdfPath); // se lo salvo ora, poi mi sparisce la prima pagina! (rimane bianca.. bho?)
				bytes = PdfHelper.ConvertToByteArray(pdf);
				//contentBase64 = PdfHelper.ConvertToBase64(bytes);
				//contentBase64 = PdfHelper.ConvertToBase64(pdf);
			}

			string contentBase64 = PdfHelper.ConvertToBase64(bytes);
			IFileMessage fileMsg = new FileMessage()
			{
				FileContentBase64 = contentBase64,
				FileName = pdfFileName
			};

			return fileMsg;
		}

		//-------------------------------------------------------------------------------
		private static void SavePdfToFile(byte[] bytes, string pdfFileName)
		{
			string mainPdfPath = @"C:\TEMP\TempFiles\" + pdfFileName;

			using (Stream stream = new MemoryStream(bytes))
			using (PdfDocument pdfClone = PdfReader.Open(stream))
			{
				pdfClone.Save(mainPdfPath); // ora posso serializzarla senza problemi dal byte array integro
			}
		}

		//-------------------------------------------------------------------------------
		private static PdfDocument BuildCombinedPdf(TB_MsgLots lot, string senderMultiLine, string company)
		{
			List<TB_MsgQueue> msgs = lot.GetMessages(company);

			string[] addressee = lot.Addressee.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
			string[] txts = 
				{
					addressee[0],
					addressee.Length == 1 ? string.Empty : addressee[1],
					lot.Address,
					lot.Zip + " " + lot.City + "   "  // tra Città e Provincia sono richiesti da 2 a 4 caratteri => ne metto 3
						+ lot.County.ToUpperInvariant(), // FIX real case scenario, rilevato da beta tester, in cui il doc veniva rifiutato
					lot.Country
				};

			List<string> docDescriptions = new List<string>();
			foreach (TB_MsgQueue item in msgs)
				docDescriptions.Add(string.Format(CultureInfo.CurrentCulture, 
					LocalizedStrings.AttachmentRowMask,//"{0} ({1} pagina/e)", 
					item.Subject, item.DocPages));

			PdfCoverData covData = new PdfCoverData();
			covData.FaxText = lot.EnumDeliveryType == Delivery.Fax ? TB_MsgLots.CleanFaxNumber(lot.Fax) : null;
			covData.SenderMultiRow = senderMultiLine;
			covData.AddressRows = txts;
			covData.InAttachTextLocalized = LocalizedStrings.InAttachment;// "In allegato:";
			covData.DocDescriptions = docDescriptions;
			//covData.DrawRectangles = true;
			//covData.DrawRulers = true;

			PdfDocument pdfDocMain = PdfHelper.CreateCover(covData);

			// Se fronte-retro, anche la copertina deve essere due pagine!

			// TODO - se fronte-retro, la copertina può essere di due pagine (sennò ne concatena una bianca)
			// e l'elenco dei documenti può anche spalmarsi sulla seconda pagina
			// Meglio numerare le pagine della copertina.

			bool addShims = lot.IsDoubleSided; // Fronte-retro

			int totalPdfPages = pdfDocMain.PageCount;
			foreach (TB_MsgQueue item in msgs)
			{
				if (addShims &&
					(totalPdfPages % 2 == 1)) // dispari
				{
					// aggiungi pagine bianche eventuali se necessario per FronteRetro
					pdfDocMain.AddPage(); // funziona anche per la copertina
				}

				int repPageCount;
				using (Stream stream = new MemoryStream(item.DocImage))
				using (PdfDocument pdfDoc = PdfReader.Open(stream, PdfDocumentOpenMode.Import))
				{
					repPageCount = pdfDoc.PageCount;
					for (int i = 0; i < repPageCount; i++)
					{
						PdfPage page = pdfDoc.Pages[i];
						pdfDocMain.AddPage(page);
					}
				}
				totalPdfPages += repPageCount;
			}

			return pdfDocMain;
		}

		//-------------------------------------------------------------------------------
		internal static int CalculateLotPdfPages(TB_MsgLots lot, PostaLiteIntegrationEntities db)
		{
			bool addShims = lot.IsDoubleSided; // Fronte-retro

			// prende tutti i messaggi cui non è ancora stato assegnato un lotto
			var lotMsgs = from p in db.TB_MsgQueue
					   where p.LotId == lot.LotID
					   select p;

			int totalPdfPages = addShims ? 2 : 1; // la cover è di uno o due documenti (sarà sempre vero?)
			// al max la seconda pagina della cover è bianca

			foreach (var msg in lotMsgs)
			{
				totalPdfPages = AddMessagePagesToCount(addShims, totalPdfPages, msg);
			}

			return totalPdfPages;
		}

		//-------------------------------------------------------------------------------
		public static int AddMessagePagesToCount(bool addShims, int totalPdfPages, TB_MsgQueue msg)
		{
			if (addShims &&
				(totalPdfPages % 2 == 1)) // dispari
			{
				// aggiungi pagine bianche eventuali se necessario per FronteRetro
				totalPdfPages += 1;
			}

			totalPdfPages += msg.DocPages;
			return totalPdfPages;
		}
	}
}
