using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace Microarea.TaskBuilderNet.TbSenderBL.Pdf
{
	static public class PdfHelper
	{
		static public int GetPdfPages(byte[] pdfBytes)
		{
			using (Stream stream = new MemoryStream(pdfBytes))
			using (PdfDocument pdfDoc = PdfReader.Open(stream, PdfDocumentOpenMode.InformationOnly))
			{
				return pdfDoc.PageCount;
			}
		}

		public static PdfDocument CreateCover(PdfCoverData covData)
		{
			bool drawRectangles = covData.DrawRectangles;
			bool drawRulers = covData.DrawRulers;

			string faxTxt = covData.FaxText;
			string[] txts = covData.AddressRows;
			string sender = covData.SenderMultiRow;
			List<string> docDescriptions = covData.DocDescriptions;
			string locInAttachText = covData.InAttachTextLocalized;

			// al momento l'unica unità di misura supportata da PdfSharp è Point

			double fontHeight = 9.0D; // points

			XRect faxRect = new XRect()
			{
				X = XUnit.FromMillimeter(110.0D),
				Y = XUnit.FromMillimeter(11.0D),
				Width = XUnit.FromMillimeter(201.0D - 110.0D),
				Height = fontHeight
			};
			XRect faxTagRect = new XRect()
			{
				X = XUnit.FromMillimeter(110.0D),
				Y = XUnit.FromMillimeter(11.0D),
				Width = XUnit.FromMillimeter(10.0D),
				Height = fontHeight
			};
			XRect faxTxtRect = new XRect()
			{
				X = XUnit.FromMillimeter(122.0D),//110+10+2
				Y = XUnit.FromMillimeter(11.0D),
				Width = XUnit.FromMillimeter(201.0D - 110.0D - 12.0D),
				Height = fontHeight
			};

			XRect sndRect = new XRect()
			{
				X = XUnit.FromMillimeter(11.0D),
				Y = XUnit.FromMillimeter(14.0D),
				Width = XUnit.FromMillimeter(101.0D - 11.0D),
				Height = XUnit.FromMillimeter(36.0D - 14.0D)
			};

			XRect addRect = new XRect()
			{
				X = XUnit.FromMillimeter(104.0D),
				Y = XUnit.FromMillimeter(62.0D),
				Width = XUnit.FromMillimeter(201.0D - 104.0D),
				Height = XUnit.FromMillimeter(78.0D - 62.0D)
			};
			string tagMask = "<I{0}>";
			double yStart = addRect.Y;
			XRect tagRect = new XRect()
			{
				X = XUnit.FromMillimeter(110.0D),
				Y = yStart,
				Width = XUnit.FromMillimeter(10.0D),
				Height = fontHeight
			};
			XRect txtRect = new XRect()
			{
				X = XUnit.FromMillimeter(122.0D),//110+10+2
				Y = yStart,
				Width = XUnit.FromMillimeter(201.0D - 110.0D - 12.0D),
				Height = fontHeight
			};

			XFont font = new XFont("Arial", fontHeight, XFontStyle.Regular);

			PdfDocument pdfDoc = new PdfDocument();
			PdfPage page = pdfDoc.AddPage();
			page.Size = PdfSharp.PageSize.A4;

			using (XGraphics gfx = XGraphics.FromPdfPage(page))
			{
				//gfx.PageUnit = XGraphicsUnit.Millimeter;

				if (drawRectangles)
				{
					DrawEmptyRectangle(gfx, sndRect);
					DrawEmptyRectangle(gfx, faxRect);
					DrawEmptyRectangle(gfx, addRect);

					DrawEmptyRectangle(gfx, faxTagRect);
					DrawEmptyRectangle(gfx, faxTxtRect);
					DrawEmptyRectangle(gfx, tagRect);
					DrawEmptyRectangle(gfx, txtRect);
				}

				bool isFax = false == string.IsNullOrEmpty(faxTxt);
				if (isFax)
				{
					string faxTag = "<FAX>";
					DrawTaggedLineAddress(faxTagRect, faxTxtRect, font, gfx, faxTag, faxTxt);
				}
				DrawSenderField(sender, sndRect, font, gfx);
				DrawAddressFields(fontHeight, txts, tagMask, yStart, tagRect, txtRect, font, gfx, isFax);
				DrawDocumentsList(docDescriptions, locInAttachText, font, gfx);

				if (drawRulers)
				{
					DrawRulerH(gfx, font);
					DrawRulerV(gfx, font);
				}
			}

			return pdfDoc;
		}

		private static void DrawDocumentsList
			(
			List<string> docDescriptions, 
			string locInAttachText,
			XFont font, 
			XGraphics gfx)
		{
			double x = XUnit.FromMillimeter(40.0D);
			double y = XUnit.FromMillimeter(100.0D);

			gfx.DrawString(locInAttachText, font, XBrushes.Black, x, y);
			y += font.Height - 1;

			for (int i = 0; i < docDescriptions.Count; i++)
			{
				y += font.Height - 1;
				gfx.DrawString(" - " + docDescriptions[i], font, XBrushes.Black, x, y);
			}
		}

		private static void DrawSenderField(string sender, XRect sndRect, XFont font, XGraphics gfx)
		{
			double shim = XUnit.FromMillimeter(2.0D);
			string[] toks = sender.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < toks.Length; ++i)
			{
				gfx.DrawString(toks[i], font, XBrushes.Black,
					sndRect.X + shim, // metto un po' di spazio x non finire al limite della finestrella
					sndRect.Y + font.Height * (i+1));
			}
		}

		private static void DrawAddressFields(double fontHeight, string[] txts, string tagMask, double yStart, XRect tagRect, XRect txtRect, XFont font, XGraphics gfx, bool isFax)
		{
			for (int i = 0; i < txts.Length; i++)
			{
				string tag = string.Format(CultureInfo.InvariantCulture, tagMask, i);
				if (isFax)
					tag = string.Empty; // i tag I0-I3 non appaiano in caso di fax
				string txt = txts[i];
				if (i == 1 && string.IsNullOrEmpty(txt))
					continue;
				if (i == 4 &&
					(string.IsNullOrEmpty(txt) ||
					string.Compare(txt, "Italy", StringComparison.InvariantCultureIgnoreCase) == 0 ||
					string.Compare(txt, "Italia", StringComparison.InvariantCultureIgnoreCase) == 0))
					continue;
				double y = yStart + fontHeight * i;
				tagRect.Y = y;
				txtRect.Y = y;
				DrawTaggedLineAddress(tagRect, txtRect, font, gfx, tag, txt);
			}
		}

		private static void DrawEmptyRectangle(XGraphics gfx, XRect rect)
		{
			XPen pen = new XPen(XColors.Black, 1);
			pen.DashStyle = XDashStyle.Dash;
			gfx.DrawRectangle(XBrushes.YellowGreen, rect);
			gfx.DrawLines(pen, new XPoint[] { rect.TopLeft, rect.TopRight, rect.BottomRight, rect.BottomLeft, rect.TopLeft });
		}

		private static void DrawTaggedLineAddress(XRect tagRect, XRect txtRect, XFont font, XGraphics gfx, string tag, string txt)
		{
			gfx.DrawString(tag, font, XBrushes.Black, tagRect, XStringFormats.TopLeft);
			gfx.DrawString(txt, font, XBrushes.Black, txtRect, XStringFormats.TopLeft);
		}

		private static void DrawRulerH(XGraphics gfx, XFont font)
		{
			double dy = XUnit.FromMillimeter(6.0D);
			double dy2 = dy + 10.0D;
			for (int x = 10; x < 210; x += 10)
			{
				double dx = XUnit.FromMillimeter(x);
				gfx.DrawString(x.ToString(), font, XBrushes.Black, dx, dy, XStringFormats.TopLeft);
				gfx.DrawLine(XPens.Black, dx, dy, dx, dy2);
			}
		}

		private static void DrawRulerV(XGraphics gfx, XFont font)
		{
			double dx = XUnit.FromMillimeter(6.0D);
			double dx2 = dx + 10.0D;
			for (int y = 10; y < 297; y += 10)
			{
				double dy = XUnit.FromMillimeter(y);
				gfx.DrawString(y.ToString(), font, XBrushes.Black, dx, dy);
				gfx.DrawLine(XPens.Black, dx, dy, dx2, dy);
			}
		}

		public static byte[] ConvertToByteArray(PdfDocument pdfDocument)
		{
			using (MemoryStream stream = new MemoryStream())
			{
				pdfDocument.Save(stream, false);
				byte[] bytes = stream.ToArray();
				return bytes;
			}
		}

		public static string ConvertToBase64(byte[] bytes)
		{
			string base64String = Convert.ToBase64String(bytes, 0, bytes.Length);
			return base64String;
		}

		public static string ConvertToBase64(PdfDocument pdfDocument)
		{
			byte[] bytes = PdfHelper.ConvertToByteArray(pdfDocument);
			string base64 = PdfHelper.ConvertToBase64(bytes);
			return base64;
		}
	}
}
