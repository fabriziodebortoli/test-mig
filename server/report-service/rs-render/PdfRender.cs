using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;

using PdfSharp.Drawing.Layout;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

using TaskBuilderNetCore.Interfaces;

using Microarea.Common.Applications;
using Microarea.Common.CoreTypes;

using Microarea.RSWeb.WoormViewer;
using Microarea.RSWeb.Objects;
using Microarea.Common.Temp;
using Microarea.RSWeb.WoormWebControl;
using Microarea.Common.NameSolver;

namespace Microarea.RSWeb.Render
{

    // <summary>
    // Descrizione di riepilogo per WoormWebControl.
    // </summary>
    // ================================================================================
    public class PdfRender
    {
        PdfDocument document = new PdfDocument();
        private WoormDocument woorm;

        public PdfRender(WoormDocument woorm)
        {
            this.woorm = woorm;
        }


        //--------------------------------------------------------------------------
        public void ReportPage()
        {

            try
            {
                PdfPage page = document.AddPage();

                page.Width = XUnit.FromMillimeter(woorm.PageInfo.DmPaperWidth / 10).Point;
                page.Height = XUnit.FromMillimeter(woorm.PageInfo.DmPaperLength / 10).Point;

                XGraphics xg = XGraphics.FromPdfPage(page);

                // incollo al panel l'eventuale immagine di sfondo
                if (woorm.Options.BkgnBitmap != null && woorm.Options.BkgnBitmap.Length != 0)
                {
                    string sourceFilePath = woorm.GetFilename(woorm.Options.BkgnBitmap, NameSpaceObjectType.Image);
                    if (PathFinder.PathFinderInstance.ExistFile(sourceFilePath))
                    {
                        //TODO RSWEB
                        //Image image = ImagesHelper.LoadImageWithoutLockFile(sourceFilePath);
                        //Rectangle imageRect = new Rectangle(woorm.Options.BitmapOrigin, image.Size);
                        //xg.DrawImage(XImage.FromGdiPlusImage(image), ScaleFromWoorm(imageRect));
                    }
                }

                RenderBaseObjList(xg, woorm.Objects);

            }
            catch (Exception)
            {

            }
        }

        private void RenderBaseObjList(XGraphics xg, BaseObjList list)
        {
            foreach (BaseObj obj in list)
            {
                switch (obj.GetType().Name)
                {
                    case "FieldRect": FieldRectPdf(xg, (FieldRect)obj); break;
                    case "TextRect": TextRectPdf(xg, (TextRect)obj); break;
                    case "GraphRect": GraphRectPdf(xg, (GraphRect)obj); break;
                    case "SqrRect": SqrRectPdf(xg, (SqrRect)obj, ((SqrRect)obj).DynamicBkgColor); break;
                    case "FileRect": FileRectPdf(xg, (FileRect)obj); break;

                    case "Table":
                        TablePdf(xg, (Microarea.RSWeb.Objects.Table)obj);
                        break;

                    case "Repeater":
                        RepeaterPdf(xg, (Repeater)obj);
                        break;

                    case "MetafileRect": break;
                    default:
                        UnknowPdf(xg);
                        break;
                }
            }
        }

        //------------------------------------------------------------------------------
        private void RepeaterPdf(XGraphics xg, Repeater repeater)
        {
            SqrRectPdf(xg, repeater, repeater.DynamicBkgColor);

            foreach (BaseObjList list in repeater.Rows)
            {
                RenderBaseObjList(xg, list);
            }
        }

        //------------------------------------------------------------------------------
        private void UnknowPdf(XGraphics xg)
        {

        }

        //------------------------------------------------------------------------------
        private void TablePdf(XGraphics xg, Table obj)
        {
            // valuata dal motore del viewer durante il parse.
            if (obj.IsHidden) return;

            WriteTableTitlePdf(xg, obj);
            WriteColumnsTitlePdf(xg, obj);
            WriteTableBodyPdf(xg, obj);
            WriteTableTotalsPdf(xg, obj);

            //disegno eventuale ombra
            WriteDropShadowPdf(xg, obj.BaseCellsRect, obj.DropShadowHeight, obj.DropShadowColor, obj.TitlePen);
        }


        ///<summary>
        ///Metodo statico che dato un rettongolo di un oggetto, e il suo XGraphics, disegna due rettangoli in basso e a 
        ///destra per dare l'effetto ombra all'oggetto
        ///</summary>
        //------------------------------------------------------------------------------
        private static void WriteDropShadowPdf(XGraphics xg, Rectangle rectangleObj, int shadowHeight, Color color, BorderPen borderPen)
        {
            //se c'e' ombra chiamo il metodo che la disegna
            if (shadowHeight <= 0)
                return;
            double w = XUnit.FromMillimeter(Scale(borderPen.Width, 3)).Point;

            XBrush brush = new XSolidBrush(new XColor(color));

            XRect bottomShadow = ScaleFromWoorm(new Rectangle(rectangleObj.Left + shadowHeight, rectangleObj.Bottom + borderPen.Width, rectangleObj.Width, shadowHeight));
            xg.DrawRectangle(brush, bottomShadow);

            XRect rightShadow = ScaleFromWoorm(new Rectangle(rectangleObj.Right + borderPen.Width, rectangleObj.Top + shadowHeight, shadowHeight, rectangleObj.Height));
            xg.DrawRectangle(brush, rightShadow);
        }

        //------------------------------------------------------------------------------
        private void WriteTableTotalsPdf(XGraphics xg, Table obj)
        {
            int lastColumn = obj.LastVisibleColumn();
            for (int col = 0; col <= lastColumn; col++)
            {
                Column column = (Column)obj.Columns[col];

                if (!column.IsHidden)
                {
                    TotalCell total = column.TotalCell;
                    bool first = (col == 0);
                    bool last = (col == obj.ColumnNumber - 1);

                    int nextVisibleColumn = obj.NextVisibleColumn(col);

                    bool nextColumnHasTotal =
                        (
                        (col < lastColumn) &&
                        (nextVisibleColumn >= 0) &&
                        obj.HasTotal(nextVisibleColumn)
                        );
                    BorderPen pen = total.TotalPen;
                    Borders borders;
                    if (column.ShowTotal)
                    {
                        borders = new Borders
                            (
                            false,
                            first,
                            obj.Borders.Total.Bottom,
                            obj.Borders.Total.Right
                            );
                    }
                    else
                    {   // serve per scrivere il bordo del successivo totale
                        borders = new Borders
                            (
                            false,
                            false,
                            false,
                            !last && nextColumnHasTotal && obj.Borders.Total.Left
                            );
                        //disegno il bordo sx del prossimo totale con il suo Pen e non con quello della cella corrente
                        //(allinemento con comportamento di woorm c++)
                        if (col + 1 <= lastColumn)
                        {
                            Column nextColumn = (Column)obj.Columns[col + 1];
                            pen = nextColumn.TotalCell.TotalPen;
                        }
                    }

                    Color fore = woorm.TrueColor(BoxType.Total, total.DynamicTotalTextColor, total.Value.FontStyleName);
                    Color bkg = !column.ShowTotal || obj.Transparent ? Color.FromArgb(0, 255, 255, 255) : total.DynamicTotalBkgColor;

                    if (total.HasFormatStyleExpr)
                    {
                        string formatStyleName = total.DynamicFormatStyleName;
                        if (formatStyleName.Length > 0 && total.Value.RDEData != null)
                        {
                            total.Value.FormattedData = this.woorm.FormatFromSoapData(formatStyleName, column.InternalID, total.Value.RDEData);
                        }
                    }

                    WriteSingleCell
                    (
                        xg,
                        total.RectCell,
                        borders, pen,
                        fore, bkg,
                        total.Value.FontStyleName,
                        total.Value.Align,
                        column.ShowTotal ? total.Value.FormattedData : "",
                        false,
                        null, null
                    );
                }
            }
        }

        //------------------------------------------------------------------------------
        private void WriteTableBodyPdf(XGraphics xg, Table obj)
        {
            //predispone la table per la modalita di Easyview dinamica (nel caso sia presente)
            obj.InitEasyview();

            int lastColumn = obj.LastVisibleColumn();
            for (int row = 0; row < obj.RowNumber; row++)
            {
                bool firstCol = true;
                for (int col = 0; col <= lastColumn; col++)
                {
                    Column column = (Column)obj.Columns[col];
                    if (!column.IsHidden)
                    {
                        if (row == 0)
                            column.PreviousValue = null;

                        Cell cell = (Cell)column.Cells[row];
                        bool lastCol = col == lastColumn;
                        cell.AtRowNumber = row;

                        if (cell.HasFormatStyleExpr)
                        {
                            string formatStyleName = cell.DynamicFormatStyleName;
                            if (formatStyleName.Length > 0 && cell.Value.RDEData != null)
                            {
                                cell.Value.FormattedData = woorm.FormatFromSoapData(formatStyleName, column.InternalID, cell.Value.RDEData);
                            }
                        }

                        Borders borders = new Borders
                            (
                            false,
                            firstCol && obj.Borders.Body.Left,
                            obj.HasBottomBorderAtCell(cell),
                            (!lastCol && obj.Borders.ColumnSeparator) || (lastCol && obj.Borders.Body.Right)
                            );
                        Borders cellBorders = cell.DynamicCellBorders(borders);

                        //--------------------------------------
                        string fontStyleName = string.Empty;
                        if (!cell.SubTotal && cell.HasTextFontStyleExpr)
                        {
                            fontStyleName = cell.DynamicTextFontStyleName;
                        }
                        if (fontStyleName.Length == 0)
                            fontStyleName = cell.SubTotal
                                ? column.SubTotal.FontStyleName
                                : cell.Value.FontStyleName;
                        //---------------------------------------
                        Color fore = cell.SubTotal
                            ? woorm.TrueColor(BoxType.SubTotal, cell.DynamicSubTotalTextColor, fontStyleName)
                            : woorm.TrueColor(BoxType.Cell, cell.DynamicTextColor, fontStyleName);

                        Color bkg = cell.SubTotal
                            ?
                                cell.GetDynamicSubTotalBkgColor (obj.UseColorEasyview(row) ? obj.EasyviewColor : cell.TemplateSubTotalBkgColor)
                            :
                                cell.GetDynamicBkgColor         (obj.UseColorEasyview(row) ? obj.EasyviewColor : cell.TemplateBkgColor);

                        if (obj.FiscalEnd && row >= obj.CurrentRow)
                        {
                            fore = Color.FromArgb(255, 112, 128, 144);
                            bkg = Color.FromArgb(255, 105, 105, 105);
                        }

                        if (cell.SubTotal)
                        {
                            column.PreviousValue = null;

                            WriteSingleCell
                            (
                                xg,
                                cell.RectCell,
                                cellBorders, column.ColumnPen,
                                fore,
                                bkg,
                                fontStyleName,
                                column.SubTotal.Align,
                                cell.Value.FormattedData,
                                false,
                                column, cell
                            );
                        }
                        else
                        {
                            WriteSingleCell
                           (
                               xg,
                               cell.RectCell,
                               cellBorders, column.ColumnPen,
                               fore, bkg,
                               fontStyleName,
                               cell.Value.Align,
                               cell.FormattedDataForWrite,
                               false,
                               column, cell
                           );
                        }

                        firstCol = false;
                    }
                }
                obj.EasyViewNextRow(row);
            }
        }

        //------------------------------------------------------------------------------
        private void WriteColumnsTitlePdf(XGraphics xg, Table table)
        {
            bool firstColumn = true;
            int lastColumn = table.LastVisibleColumn();

            for (int col = 0; col <= lastColumn; col++)
            {
                Column column = table.Columns[col];
                bool showTitle = ((table.HideTableTitle && table.HideColumnsTitle) || table.HideColumnsTitle);
                bool last = col == lastColumn;

                if (!column.IsHidden)
                {
                    Borders borders = new Borders
                    (
                        table.Borders.ColumnTitle.Top && !showTitle,
                        firstColumn && table.Borders.ColumnTitle.Left && !showTitle,
                        table.Borders.Body.Top,
                        !showTitle &&
                        (
                        (last && table.Borders.ColumnTitle.Right) ||
                        (!last && table.Borders.ColumnSeparator && table.Borders.ColumnTitleSeparator)
                        )
                    );

                    WriteSingleCell
                    (
                        xg, column.ColumnTitleRect,
                        borders, column.ColumnTitlePen,
                        woorm.TrueColor(BoxType.ColumnTitle, column.DynamicTitleTextColor, column.Title.FontStyleName),
                        column.DynamicTitleBkgColor,
                        column.Title.FontStyleName,
                        column.Title.Align, column.DynamicTitleLocalizedText,
                        showTitle, null, null
                    );

                    firstColumn = false;
                }
            }
        }

        //------------------------------------------------------------------------------
        private void WriteTableTitlePdf(XGraphics xg, Table obj)
        {
            Borders borders = new Borders
            (
                obj.Borders.TableTitle.Top,
                obj.Borders.TableTitle.Left,
                false,
                obj.Borders.TableTitle.Right
            );

            WriteSingleCell
            (
                xg,
                obj.TitleRect,
                borders,
                obj.TitlePen,
                woorm.TrueColor(BoxType.TableTitle, obj.Title.TextColor, obj.Title.FontStyleName),
                obj.Transparent ? Color.FromArgb(0, 0, 0, 0) : obj.Title.BkgColor,
                obj.Title.FontStyleName,
                obj.Title.Align,
                obj.LocalizedText,
                obj.HideTableTitle,
                null,
                null
            );
        }

        //------------------------------------------------------------------------------
        private void WriteSingleCell
            (
            XGraphics xg,
            Rectangle cellRect,
            Borders borders,
            BorderPen borderPen,
            Color fontColor,
            Color backColor,
            string fontName,
            AlignType align,
            string text,
            bool hide,
            Column column,      // info per barcode
            Cell columnCell // info per barcode
            )
        {
            if (hide) return;

            XBrush brush = (new XSolidBrush(new XColor(backColor)));
            Rectangle inside = BaseRect.CalculateInsideRect(cellRect, borders, borderPen);
            xg.DrawRectangle(brush, ScaleFromWoorm(inside));

            if (column != null && columnCell != null && column.IsBarCode && columnCell.Value.FormattedData != string.Empty)
            {
                BarCodeViewer bcv = new BarCodeViewer(woorm, column.BarCode);
                string humanText = string.Empty;
                if (column.BarCode != null && column.BarCode.HumanTextAlias > 0)
                    humanText = woorm.GetFormattedDataFromAlias(column.BarCode.HumanTextAlias, columnCell.AtRowNumber);

                string barCodeFile = bcv.GetBarcodeImageFile(columnCell.Value, woorm.GetFontElement(fontName), cellRect, humanText);
                if (!String.IsNullOrEmpty(barCodeFile))
                {
                    using (Image img = Image.FromFile(bcv.GenericTmpFile))
                    {
                        WriteBarCodeXImage(xg, XImage.FromGdiPlusImage(img), cellRect);
                    }
                    PathFinder.PathFinderInstance.RemoveFile(bcv.GenericTmpFile);

                }
                else
                    DrawText(xg,
                        text,
                        inside,
                        fontName,
                        fontColor,
                        align
                    );
            }
            else if (column != null && columnCell != null && column.ShowAsBitmap)
            {
                string sourceFilePath = woorm.GetFilename(text, NameSpaceObjectType.Image);
                if (PathFinder.PathFinderInstance.ExistFile(sourceFilePath))        //copio il file sotto webfolder esposta (necessario per problemi d permessi)
                {
                    WriteXImage(xg, XImage.FromFile(sourceFilePath), cellRect, columnCell.Value.Align);
                }
                else
                    DrawText(xg,
                        text,
                        inside,
                        fontName,
                        fontColor,
                        align
                    );
            }
            else
                DrawText(xg,
                        text,
                        inside,
                        fontName,
                        fontColor,
                        align
                    );
            WriteBorders(xg, borderPen, cellRect, borders);
        }

        //------------------------------------------------------------------------------
        private void WriteGenericImage(XGraphics xg, string file)
        {

        }

        //------------------------------------------------------------------------------
        private void GraphRectPdf(XGraphics xg, GraphRect obj)
        {
            // valuata dal motore del viewer durante il parse.
            if (obj.IsHidden) return;

            WriteImage(xg, obj, obj.ImageRect, AlignType.DT_CENTER);
            //disegno dei bordi
            WriteBorders(xg, obj);
            //disegno eventuale ombra
            WriteDropShadowPdf(xg, obj.Rect, obj.DropShadowHeight, obj.DropShadowColor, obj.BorderPen);
        }

        //------------------------------------------------------------------------------
        private void FileRectPdf(XGraphics xg, FileRect obj)
        {
            // valuata dal motore del viewer durante il parse.
            if (obj.IsHidden) return;

            DrawText(xg,
                        obj.LocalizedText,
                        obj.Rect,
                        obj.Label.FontStyleName,
                        obj.DynamicTextColor,
                        obj.Label.Align
                );

            WriteBorders(xg, obj);

            //disegno eventuale ombra
            WriteDropShadowPdf(xg, obj.Rect, obj.DropShadowHeight, obj.DropShadowColor, obj.BorderPen);
        }

        //------------------------------------------------------------------------------
        private void DrawText(XGraphics xg, string text, Rectangle rect, string fontStyleName, Color color, AlignType align)
        {
            if (String.IsNullOrWhiteSpace(text))
                return;

            text = text.Replace("\\n", "\r\n");

            //Se non e' multiline come stile di allineamento, e contiene dei "new line", li elimino per evitare che vengano
            //stampati a video dei quadratini
            if (!Helper.IsMultilineString(text, align))
                text = text.Replace("\r\n", " ");

            XBrush brush = new XSolidBrush(new XColor(woorm.TrueColor(BoxType.Text, color, fontStyleName)));

            XTextFormatterExtended formatter = new XTextFormatterExtended(xg);
            formatter.Text = text;
            formatter.Font = GetFont(fontStyleName);
            formatter.LayoutRectangle = ScaleFromWoorm(rect);

            if ((align & AlignType.DT_RIGHT) == AlignType.DT_RIGHT)
                formatter.Alignment = XParagraphAlignment.Right;
            else if ((align & AlignType.DT_CENTER) == AlignType.DT_CENTER)
                formatter.Alignment = XParagraphAlignment.Center;
            else
                formatter.Alignment = XParagraphAlignment.Left;

            double fontHeight = formatter.Font.GetHeight();
            double rectHeight = formatter.LayoutRectangle.Height;
            XRect r = formatter.LayoutRectangle;

            formatter.DrawStringExtended(xg, formatter, brush, r, align);
        }

        //------------------------------------------------------------------------------
        private static string TruncateLongString(XGraphics xg, string text, XFont font, XRect xRect, XStringFormat format, bool alignRight)
        {
            XSize sz = xg.MeasureString(text, font, format);
            while (sz.Width > xRect.Width)
            {
                text = alignRight
                    ? text.Substring(1, text.Length - 1)
                    : text.Substring(0, text.Length - 1);
                sz = xg.MeasureString(text, font);
            }

            return text;
        }

        //------------------------------------------------------------------------------
        internal void SaveToStreamAndClose(Stream stream, bool closeStream)
        {
            document.Save(stream, closeStream);
            document.Close();
        }

        //------------------------------------------------------------------------------
        internal void SaveToFileAndClose(string file)
        {
            document.Save(file);
            document.Close();
        }

        //-----------------------------------------------------------------------------
        public void WriteXImage(XGraphics xg, XImage img, Rectangle rect, AlignType align)
        {
            XRect xRect = ScaleFromWoorm(rect);
            XRect originalRect = xRect;
            //calcolo le dimensioni dell'immagine in modo che sfrutti al massimo lo spazio del contenitore
            double hRatio = xRect.Width / img.PixelWidth;
            double vRatio = xRect.Height / img.PixelHeight;

            double width, height;
            if (hRatio > vRatio)
            {
                width = img.PixelWidth * vRatio;
                height = img.PixelHeight * vRatio;
            }
            else
            {
                width = img.PixelWidth * hRatio;
                height = img.PixelHeight * hRatio;
            }
            xRect.Width = width;
            xRect.Height = height;

            if ((align & AlignType.DT_RIGHT) == AlignType.DT_RIGHT)
                xRect.Offset(originalRect.Width - xRect.Width, 0);
            else if ((align & AlignType.DT_CENTER) == AlignType.DT_CENTER)
                xRect.Offset((originalRect.Width - xRect.Width) / 2, 0);

            if ((align & AlignType.DT_VCENTER) == AlignType.DT_VCENTER)
                xRect.Offset(0, (originalRect.Height - xRect.Height) / 2);
            else if ((align & AlignType.DT_BOTTOM) == AlignType.DT_BOTTOM)
                xRect.Offset(0, originalRect.Height - xRect.Height);

            xg.DrawImage(img, xRect);
        }

        //-----------------------------------------------------------------------------
        public void WriteBarCodeXImage(XGraphics xg, XImage img, Rectangle rect)
        {
            XRect xRect = ScaleFromWoorm(rect);
            XRect originalRect = xRect;

            //draw it with original size
            XSize szImage = ScaleFromWoorm(new Size(img.PixelWidth, img.PixelHeight));
            xRect.Width = szImage.Width;
            xRect.Height = szImage.Height;

            //center barcode inside the cell
            xRect.Offset((originalRect.Width - xRect.Width) / 2, (originalRect.Height - xRect.Height) / 2);

            xg.DrawImage(img, xRect);
        }

        //-----------------------------------------------------------------------------
        public void WriteImage(XGraphics xg, IImage obj, Rectangle rect, AlignType align)
        {
            XRect xRect = ScaleFromWoorm(rect);
            XBrush brush = (new XSolidBrush(new XColor(Color.White)));
            xg.DrawRectangle(brush, xRect);

            string sourceFilePath = woorm.GetFilename(obj.ImageFile, NameSpaceObjectType.Image);
            if (string.IsNullOrWhiteSpace(sourceFilePath) || !PathFinder.PathFinderInstance.ExistFile(sourceFilePath))
                return;
            XImage img = XImage.FromFile(sourceFilePath);
            WriteXImage(xg, img, rect, align);
        }

        //------------------------------------------------------------------------------
        private void FieldRectPdf(XGraphics xg, FieldRect obj)
        {
            // valuata dal motore del viewer durante il parse.
            if (obj.IsHidden) return;

            SqrRectPdf(xg, obj, obj.DynamicBkgColor);

            if (obj.IsImage)
            {
                // costruisce una tabella trasparente con una sola cella inflatando dei bordi
                Rectangle inflatedRect = Helper.Inflate(obj.ImageRect, obj.Borders, obj.BorderPen);
                WriteImage(xg, obj, inflatedRect, obj.Value.Align);
                return;
            }

            Rectangle labelRect = obj.Rect;
            labelRect.Inflate(-2, -2); //border from rect
            DrawText(xg, obj.DynamicLabelLocalizedText, labelRect, obj.Label.FontStyleName, obj.DynamicLabelTextColor, obj.Label.Align);

            // costruisce una tabella trasparente con una sola cella inflatando dei bordi
            Rectangle inflated = Helper.Inflate(obj.Rect, obj.Borders, obj.BorderPen);

            if (obj.IsTextFile)
                DrawText(xg,
                        ReadTextFile(obj.Value.FormattedData),
                        inflated,
                        obj.Value.FontStyleName,
                        obj.DynamicTextColor,
                        obj.Value.Align
                );
            else if (obj.IsBarCode && obj.Value.FormattedData != string.Empty)
            {
                BarCodeViewer bcv = new BarCodeViewer(woorm, obj.BarCode);
                string humanText = string.Empty;
                if (obj.BarCode != null && obj.BarCode.HumanTextAlias > 0)
                    humanText = woorm.GetFormattedDataFromAlias(obj.BarCode.HumanTextAlias);

                string barCodeFile = bcv.GetBarcodeImageFile(obj.Value, woorm.GetFontElement(obj.Value.FontStyleName), obj.InsideRect, humanText);
                if (!String.IsNullOrEmpty(barCodeFile))
                {
                    using (Image img = Image.FromFile(bcv.GenericTmpFile))
                    {
                        Rectangle inflatedRect = Helper.Inflate(obj.ImageRect, obj.Borders, obj.BorderPen);
                        inflatedRect.Inflate(-5, -5);
                        XRect xRect = ScaleFromWoorm(inflatedRect);
                        XBrush brush = (new XSolidBrush(new XColor(Color.White)));
                        xg.DrawRectangle(brush, xRect);
                        XImage xImg = XImage.FromGdiPlusImage(img);
                        WriteXImage(xg, xImg, inflatedRect, obj.Value.Align);
                    }
                    PathFinder.PathFinderInstance.RemoveFile(bcv.GenericTmpFile);
                }
                return;
            }
            else
            {
                if (obj.HasFormatStyleExpr && obj.Value.RDEData != null)
                {
                    string formatStyleName = obj.DynamicFormatStyleName;
                    if (formatStyleName.Length > 0)
                    {
                        obj.Value.FormattedData = woorm.FormatFromSoapData(formatStyleName, obj.InternalID, obj.Value.RDEData);
                    }
                }

                DrawText(xg, obj.Value.FormattedData, inflated, obj.Value.FontStyleName, obj.DynamicTextColor, obj.Value.Align);
            }
        }

        //------------------------------------------------------------------------------
        private string ReadTextFile(string textFilename)
        {
            // gestisce i Namespace o i filename tradizionali
            string filename = woorm.GetFilename(textFilename, NameSpaceObjectType.Text);
            if (PathFinder.PathFinderInstance.ExistFile(filename))
            {
                try
                {
                    return PathFinder.PathFinderInstance.GetFileTextFromFileName(filename);
                }
                catch (IOException e)
                {
                    return e.ToString();
                }
            }
            return WoormWebControlStrings.FileNotFound;
        }

        //------------------------------------------------------------------------------
        public void SqrRectPdf(XGraphics xg, BaseRect obj, Color backColor)
        {
            // valuata dal motore del viewer durante il parse.
            if (obj.IsHidden) return;
            XBrush brush = (new XSolidBrush(new XColor(backColor)));
            xg.DrawRectangle(brush, ScaleFromWoorm(obj.Rect));
            WriteBorders(xg, obj);

            //disegno eventuale ombra
            WriteDropShadowPdf(xg, obj.Rect, obj.DropShadowHeight, obj.DropShadowColor, obj.BorderPen);
        }

        //------------------------------------------------------------------------------
        private void TextRectPdf(XGraphics xg, TextRect obj)
        {
            // valuata dal motore del viewer durante il parse.
            if (obj.IsHidden) return;

            SqrRectPdf(xg, obj, obj.DynamicBkgColor);
            DrawText(xg, obj.LocalizedText, obj.Rect, obj.Label.FontStyleName, obj.DynamicTextColor, obj.Label.Align);
        }

        //------------------------------------------------------------------------------
        public static double Scale(int lpv, int decimals)
        {
            // Woorm coordinates assume 96 DPI
            const int SCALING_FACTOR = 96;

            // convert from logical units to inch
            double muv = (double)lpv / SCALING_FACTOR;

            // convert to mm
            muv *= 25.4;

            // round to nDec decimal digits
            return Math.Round(muv, decimals);

        }
        //------------------------------------------------------------------------------
        private static XRect ScaleFromWoorm(Rectangle rect)
        {
            return new XRect(
                XUnit.FromMillimeter(Scale(rect.Left, 3)).Point,
                XUnit.FromMillimeter(Scale(rect.Top, 3)).Point,
                XUnit.FromMillimeter(Scale(rect.Width, 3)).Point,
                XUnit.FromMillimeter(Scale(rect.Height, 3)).Point);
        }
        //------------------------------------------------------------------------------
        private static XPoint ScaleFromWoorm(Point p)
        {
            return new XPoint(
                XUnit.FromMillimeter(Scale(p.X, 3)).Point,
                XUnit.FromMillimeter(Scale(p.Y, 3)).Point);
        }
        //------------------------------------------------------------------------------
        private static XSize ScaleFromWoorm(Size sz)
        {
            return new XSize(
                XUnit.FromMillimeter(Scale(sz.Width, 3)).Point,
                XUnit.FromMillimeter(Scale(sz.Height, 3)).Point);
        }
        //------------------------------------------------------------------------------
        private XFont GetFont(string fontStyleName)
        {
            FontElement fe = woorm.GetFontElement(fontStyleName);
            if (fe == null) return null;
            XPdfFontOptions options = new XPdfFontOptions(PdfFontEncoding.Unicode);
            return new XFont(fe.FaceName, XUnit.FromMillimeter(Scale(fe.Size, 3)).Point, (XFontStyle)fe.FontStyle, options);
        }
        //------------------------------------------------------------------------------
        private void WriteBorders(XGraphics xg, BaseRect obj)
        {
            WriteBorders(xg, obj.BorderPen, obj.Rect, obj.Borders);
        }

        //------------------------------------------------------------------------------
        private void WriteBorders(XGraphics xg, BorderPen borderPen, Rectangle borderRect, Borders borders)
        {
            XColor c = new XColor(borderPen.Color);

            double w = XUnit.FromMillimeter(Scale(borderPen.Width, 3)).Point;
            XPen pen = new XPen(c, w);

            XRect rect = ScaleFromWoorm(borderRect);
            if (borders.Top)
                xg.DrawLine(pen, rect.TopLeft, rect.TopRight);
            if (borders.Bottom)
                xg.DrawLine(pen, rect.BottomLeft, rect.BottomRight);
            if (borders.Left)
                xg.DrawLine(pen, rect.TopLeft, rect.BottomLeft);
            if (borders.Right)
                xg.DrawLine(pen, rect.TopRight, rect.BottomRight);
        }


        //***************************************************************************//
        //CODICE IMPORTATO DA PDFSHARP 1.31.1789 PER GESTIRE L"ALLINEAMENTO VERTICALE
        //DELLE STRINGHE MULTILINE (CONTENENTI DEI \R\N)
        //Porta qui un pezzo di comportamento della classe XTextFormatter
        //****************************************************************************//

        ///Enumerativo importato da pdfsharp, file XTextFormatter.cs
        ///NON MODIFICATO: ricopiato per motivi di visibilita' (e' privato )
        internal enum BlockType
        {
            Text, Space, Hyphen, LineBreak,
        }

        ///<summary>
        ///Classe che estende XTextFormatter di pdfsharp, file XTextFormatter.cs
        ///CON AGGIUNTA DELLA CENTRATURA VERTICALE E DELLA GESTIONE PIU TOLLERANTE DEGLI A CAPO
        ///</summary>
        //====================================================================================
        internal sealed class XTextFormatterExtended : XTextFormatter
        {
            double lineSpace = 0;
            double cyAscent = 0;
            double cyDescent = 0;
            double spaceWidth = 0;
            readonly List<Block> blocks = new List<Block>();


            public XTextFormatterExtended(XGraphics xg) : base(xg)
            {
            }


            ///<summary>
            ///Metodo per scrivere le stringhe che hanno impostanto l'allineamento del testo verticale (90 o 270 gradi)
            ///</summary>
            //------------------------------------------------------------------------------
            internal void DrawRotatedString(XGraphics xg, XTextFormatter formatter, XBrush brush, XRect layoutRectangle, AlignType align)
            {
                if ((align & (AlignType.DT_EX_90 | AlignType.DT_EX_270)) == 0)
                    return; //non e' un testo ruotato

                string text = formatter.Text;
                if (text == null)
                    throw new ArgumentNullException("text");

                if (text.Length == 0)
                    return;

                XFont font = formatter.Font;
                if (font == null)
                    throw new ArgumentNullException("font");
                if (brush == null)
                    throw new ArgumentNullException("brush");

                if (Helper.IsMultilineString(text, align))
                {
                    DrawMultilineRotatedString(text, xg, font, brush, layoutRectangle, align);
                    return;
                }

                XGraphicsState state = null;
                int angle = (align & AlignType.DT_EX_90) != 0 ? -90 : -270;
                state = xg.Save();
                xg.RotateAtTransform(angle, layoutRectangle.Center);
                //inizializzo con i valori left orizzontalmente e top verticalmente
                //x e y sono logicamnte invertite perche il rettangolo e' ruotato verticalmente
                //Rotate rectangle
                XMatrix m = new XMatrix();
                m.RotateAtPrepend(angle, layoutRectangle.Center);
                layoutRectangle.Transform(m);

                //default TOP/LEFT
                double textWidth = xg.MeasureString(text, formatter.Font).Width;
                double xTextPos = 0;
                double yTextPos = 0;

                double lineVSpace = font.GetHeight();
                if ((align & AlignType.DT_EX_90) != 0)
                {
                    xTextPos = layoutRectangle.X + layoutRectangle.Width - textWidth;
                    yTextPos = layoutRectangle.Y + lineVSpace;

                    //Allineamento orizzontale
                    if ((align & AlignType.DT_RIGHT) == AlignType.DT_RIGHT)
                        yTextPos = layoutRectangle.Y + layoutRectangle.Height;
                    else if ((align & AlignType.DT_CENTER) == AlignType.DT_CENTER)
                        yTextPos = layoutRectangle.Y + layoutRectangle.Height / 2 + lineVSpace / 2;

                    //Allineamento verticale
                    if ((align & AlignType.DT_VCENTER) == AlignType.DT_VCENTER)
                        xTextPos = layoutRectangle.X + layoutRectangle.Width / 2 - textWidth / 2;
                    if ((align & AlignType.DT_BOTTOM) == AlignType.DT_BOTTOM)
                        xTextPos = (align & AlignType.DT_EX_90) != 0 ? layoutRectangle.X : layoutRectangle.X + layoutRectangle.Width - textWidth;
                }
                else //270 grdi di rotazione
                {
                    xTextPos = layoutRectangle.X;
                    yTextPos = layoutRectangle.Y + layoutRectangle.Height;

                    //Allineamento orizzontale
                    if ((align & AlignType.DT_RIGHT) == AlignType.DT_RIGHT)
                        yTextPos = layoutRectangle.Y + lineVSpace;
                    else if ((align & AlignType.DT_CENTER) == AlignType.DT_CENTER)
                        yTextPos = layoutRectangle.Y + layoutRectangle.Height / 2 + lineVSpace / 2;

                    //Allineamento verticale
                    if ((align & AlignType.DT_VCENTER) == AlignType.DT_VCENTER)
                        xTextPos = layoutRectangle.X + layoutRectangle.Width / 2 - textWidth / 2;
                    if ((align & AlignType.DT_BOTTOM) == AlignType.DT_BOTTOM)
                        xTextPos = (align & AlignType.DT_EX_90) != 0 ? layoutRectangle.X : layoutRectangle.X + layoutRectangle.Width - textWidth;
                }

                /*SCOMMENTARE PER AUSILIO VISIVO IN DEBUG*/
                /*xg.DrawRectangle(XBrushes.Aqua, layoutRectangle);
				XRect fontRect =  new XRect(xTextPos, yTextPos - lineVSpace, textWidth, lineVSpace);
				xg.DrawRectangle(XBrushes.Red, fontRect);*/

                //Attenzione al concetto di baseline e descent nel font, bisogna scrivere tenendo conto che gerte lettere (g,p ecc) scendo 
                //al di sotto della baseline

                int cellSpace = font.FontFamily.GetLineSpacing(font.Style);
                int cellDescent = font.FontFamily.GetCellDescent(font.Style);
                double descent = lineVSpace * cellDescent / cellSpace;

                xg.DrawString(text, font, brush, xTextPos, yTextPos - descent);

                if (state != null)
                    xg.Restore(state);
                return;
            }

            ///<summary>
            ///Metodo per scrivere le stringhe che hanno impostanto l'allineamento del testo verticale (90 o 270 gradi) e sono multiline
            ///</summary>
            //------------------------------------------------------------------------------
            private void DrawMultilineRotatedString(string text, XGraphics xg, XFont font, XBrush brush, XRect layoutRectangle, AlignType align)
            {
                string[] lines = Helper.SplitMultilineString(text);
                XGraphicsState state = null;
                int angle1 = (align & AlignType.DT_EX_90) != 0 ? -90 : -270;
                state = xg.Save();
                xg.RotateAtTransform(angle1, layoutRectangle.Center);
                //inizializzo con i valori left orizzontalmente e top verticalmente
                //x e y sono logicamnte invertite perche il rettangolo e' ruotato verticalmente
                //Rotate rectangle
                XMatrix m = new XMatrix();
                m.RotateAtPrepend(angle1, layoutRectangle.Center);
                layoutRectangle.Transform(m);

                //default TOP/LEFT
                double textWidth = 0;
                int numLines = lines.Length;

                //compute line with maximun width
                foreach (string line in lines)
                {
                    double lineWidth = xg.MeasureString(text, font).Width;
                    if (textWidth < lineWidth)
                        textWidth = lineWidth;
                }

                double xTextPos = 0;
                double yTextPos = 0;

                double lineVSpace = font.GetHeight();
                double textVSpace = lineVSpace * numLines;
                if ((align & AlignType.DT_EX_90) != 0)
                {
                    xTextPos = layoutRectangle.X + layoutRectangle.Width - textWidth;
                    yTextPos = layoutRectangle.Y + lineVSpace;

                    //Allineamento orizzontale
                    if ((align & AlignType.DT_RIGHT) == AlignType.DT_RIGHT)
                        yTextPos = layoutRectangle.Y + layoutRectangle.Height - textVSpace + lineVSpace;
                    else if ((align & AlignType.DT_CENTER) == AlignType.DT_CENTER)
                        yTextPos = layoutRectangle.Y + layoutRectangle.Height / 2 - textVSpace / 2 + lineVSpace;

                    //Allineamento verticale
                    if ((align & AlignType.DT_VCENTER) == AlignType.DT_VCENTER)
                        xTextPos = layoutRectangle.X + layoutRectangle.Width / 2 - textWidth / 2;
                    if ((align & AlignType.DT_BOTTOM) == AlignType.DT_BOTTOM)
                        xTextPos = (align & AlignType.DT_EX_90) != 0 ? layoutRectangle.X : layoutRectangle.X + layoutRectangle.Width - textWidth;
                }
                else //270 gradi di rotazione
                {
                    xTextPos = layoutRectangle.X;
                    yTextPos = layoutRectangle.Y + layoutRectangle.Height - textVSpace + lineVSpace;

                    //Allineamento orizzontale
                    if ((align & AlignType.DT_RIGHT) == AlignType.DT_RIGHT)
                        yTextPos = layoutRectangle.Y + lineVSpace;
                    else if ((align & AlignType.DT_CENTER) == AlignType.DT_CENTER)
                        yTextPos = layoutRectangle.Y + layoutRectangle.Height / 2 - textVSpace / 2 + lineVSpace;

                    //Allineamento verticale
                    if ((align & AlignType.DT_VCENTER) == AlignType.DT_VCENTER)
                        xTextPos = layoutRectangle.X + layoutRectangle.Width / 2 - textWidth / 2;
                    if ((align & AlignType.DT_BOTTOM) == AlignType.DT_BOTTOM)
                        xTextPos = (align & AlignType.DT_EX_90) != 0 ? layoutRectangle.X : layoutRectangle.X + layoutRectangle.Width - textWidth;
                }

                /*SCOMMENTARE PER AUSILIO VISIVO IN DEBUG*/
                /*xg.DrawRectangle(XBrushes.Aqua, layoutRectangle);
				XRect fontRect =  new XRect(xTextPos, yTextPos - lineVSpace, textWidth, lineVSpace);
				xg.DrawRectangle(XBrushes.Red, fontRect);*/

                //Attenzione al concetto di baseline e descent nel font, bisogna scrivere tenendo conto che gerte lettere (g,p ecc) scendo 
                //al di sotto della baseline

                int cellSpace = font.FontFamily.GetLineSpacing(font.Style);
                int cellDescent = font.FontFamily.GetCellDescent(font.Style);
                double descent = lineVSpace * cellDescent / cellSpace;

                double y = yTextPos - descent;
                foreach (string line in lines)
                {
                    xg.DrawString(line, font, brush, xTextPos, y);
                    y += font.GetHeight();
                }
                if (state != null)
                    xg.Restore(state);
            }

            ///<summary>
            ///Metodo importato da pdfsharp, file XTextFormatter.cs
            ///CON AGGIUNTA DELLA CENTRATURA VERTICALE
            ///</summary>
            //------------------------------------------------------------------------------
            internal void DrawStringExtended(XGraphics xg, XTextFormatter formatter, XBrush brush, XRect layoutRectangle, AlignType align)
            {
                string text = formatter.Text;
                XFont font = formatter.Font;

                if (text == null)
                    throw new ArgumentNullException("text");
                if (font == null)
                    throw new ArgumentNullException("font");
                if (brush == null)
                    throw new ArgumentNullException("brush");

                if (text.Length == 0)
                    return;

                if ((align & (AlignType.DT_EX_90 | AlignType.DT_EX_270)) != 0)
                {
                    DrawRotatedString(xg, formatter, brush, layoutRectangle, align);
                    return;
                }
                lineSpace = font.GetHeight();
                cyAscent = lineSpace * font.FontFamily.GetCellAscent(font.Style) / font.FontFamily.GetLineSpacing(font.Style);
                cyDescent = lineSpace * font.FontFamily.GetCellDescent(font.Style) / font.FontFamily.GetLineSpacing(font.Style);
                spaceWidth = xg.MeasureString("x x", font).Width;
                spaceWidth -= xg.MeasureString("xx", font).Width;

                CreateBlocks(xg, formatter);

                CreateLayout(formatter);


                int count = this.blocks.Count;
                //BEGIN FIX-IMPLEMENTAZIONE: centratura verticale
                double fullTextHeight = lineSpace * GetNumLine(); //altezza complessiva nel caso il testo vada su piu linee

                if ((align & AlignType.DT_VCENTER) == AlignType.DT_VCENTER)
                    layoutRectangle.Offset(0, (layoutRectangle.Height - fullTextHeight) / 2.0);
                if ((align & AlignType.DT_BOTTOM) == AlignType.DT_BOTTOM)
                    layoutRectangle.Offset(0, layoutRectangle.Height - fullTextHeight);
                //END FIX

                double dx = layoutRectangle.Location.X;
                double dy = layoutRectangle.Location.Y + cyAscent;

                for (int idx = 0; idx < count; idx++)
                {
                    Block block = (Block)this.blocks[idx];
                    if (block.Stop)
                        break;
                    if (block.Type == BlockType.LineBreak)
                        continue;

                    xg.DrawString(block.Text, font, brush, dx + block.Location.X, dy + block.Location.Y);
                }
            }

            //Restituisce il numero di linee di cui e' composto il testo da formattare
            //------------------------------------------------------------------------------
            private int GetNumLine()
            {
                int count = 0;
                blocks.ForEach(delegate (Block block)
                            {
                                if (block.Type == BlockType.LineBreak)
                                    count++;
                            });
                return count + 1; //se ci sono n "a capo", le linee sono n+1
            }

            ///<summary>
            ///Metodo importato da pdfsharp, file XTextFormatter.cs
            ///CON AGGIUNTA DELLA GESTIONE DEGLI "A CAPO" + TOLLERANTE
            ///</summary>
            //------------------------------------------------------------------------------
            private void CreateBlocks(XGraphics xg, XTextFormatter formatter)
            {
                string text = formatter.Text;
                this.blocks.Clear();
                int length = text.Length;
                bool inNonWhiteSpace = false;
                int startIndex = 0, blockLength = 0;
                for (int idx = 0; idx < length; idx++)
                {
                    char ch = text[idx];

                    // Treat CR and CRLF as LF
                    if (ch == Helper.Chars.CR)
                    {
                        if (idx < length - 1 && text[idx + 1] == Helper.Chars.LF)
                            idx++;
                        ch = Helper.Chars.LF;
                    }
                    //OKKIO: BUG FIX X ALLINEARE PDFSHARP A WOORM (In woorm e' considerato come singolo a capo la sequenza errata \n\r(LF-CR) mentre 
                    //pdfsharp la considera come due a capo. Per il fatto che molti report anche personalizzati la usano errata, si e' deciso di farla "digerire"
                    //anche a PdfSharp)
                    if (ch == Helper.Chars.LF)
                    {
                        if (idx < length - 1 && text[idx + 1] == Helper.Chars.CR)
                            idx++;
                        ch = Helper.Chars.CR;
                    }
                    //
                    if (ch == Helper.Chars.LF || ch == Helper.Chars.CR)
                    {
                        if (blockLength != 0)
                        {
                            string token = text.Substring(startIndex, blockLength);
                            this.blocks.Add(new Block(token, BlockType.Text,
                              xg.MeasureString(token, formatter.Font).Width));
                        }
                        startIndex = idx + 1;
                        blockLength = 0;
                        this.blocks.Add(new Block(BlockType.LineBreak));
                    }
                    else if (Char.IsWhiteSpace(ch))
                    {
                        if (inNonWhiteSpace)
                        {
                            string token = text.Substring(startIndex, blockLength);
                            this.blocks.Add(new Block(token, BlockType.Text,
                              xg.MeasureString(token, formatter.Font).Width));
                            startIndex = idx + 1;
                            blockLength = 0;
                        }
                        else
                        {
                            blockLength++;
                        }
                    }
                    else
                    {
                        inNonWhiteSpace = true;
                        blockLength++;
                    }
                }
                if (blockLength != 0)
                {
                    string token = text.Substring(startIndex, blockLength);
                    this.blocks.Add(new Block(token, BlockType.Text,
                      xg.MeasureString(token, formatter.Font).Width));
                }
            }

            ///<summary>
            ///Metodo importato da pdfsharp, file XTextFormatter.cs
            ///NON MODIFICATO: ricopiato per motivi di visibilita' (e' privato nella classe padre XTextFormatter)
            ///</summary>
            //------------------------------------------------------------------------------
            private void CreateLayout(XTextFormatter formatter)
            {
                double rectWidth = formatter.LayoutRectangle.Width;
                double rectHeight = formatter.LayoutRectangle.Height - this.cyAscent - this.cyDescent;
                int firstIndex = 0;
                double x = 0, y = 0;
                int count = this.blocks.Count;
                for (int idx = 0; idx < count; idx++)
                {
                    Block block = (Block)this.blocks[idx];
                    if (block.Type == BlockType.LineBreak)
                    {
                        if (formatter.Alignment == XParagraphAlignment.Justify)
                            ((Block)this.blocks[firstIndex]).Alignment = XParagraphAlignment.Left;
                        AlignLine(formatter.Alignment, firstIndex, idx - 1, rectWidth);
                        firstIndex = idx + 1;
                        x = 0;
                        y += lineSpace /*FIX!! in pdfsharp non divide per due e lascia una linea vuota di troppo tra una riga e l'altra*/;
                    }
                    else
                    {
                        double width = block.Width; //!!!modTHHO 19.11.09 don't add this.spaceWidth here
                        if ((x + width <= rectWidth || x == 0) && block.Type != BlockType.LineBreak)
                        {
                            block.Location = new XPoint(x, y);
                            x += width + spaceWidth; //!!!modTHHO 19.11.09 add this.spaceWidth here
                        }
                        else
                        {
                            AlignLine(formatter.Alignment, firstIndex, idx - 1, rectWidth);
                            firstIndex = idx;
                            y += lineSpace /*FIX!! in pdfsharp non divide per due e lascia una linea vuota di troppo tra una riga e l'altra*/;
                            if (y > rectHeight)
                            {
                                block.Stop = true;
                                break;
                            }
                            block.Location = new XPoint(0, y);
                            x = width + spaceWidth; //!!!modTHHO 19.11.09 add this.spaceWidth here
                        }
                    }
                }
                if (firstIndex < count && formatter.Alignment != XParagraphAlignment.Justify)
                    AlignLine(formatter.Alignment, firstIndex, count - 1, rectWidth);
            }

            /// <summary>
            /// Align center, right or justify.
            /// Metodo importato da pdfsharp, file XTextFormatter.cs
            /// NON MODIFICATO: ricopiato per motivi di visibilita' (e' privato nella classe padre XTextFormatter)
            /// </summary>
            //------------------------------------------------------------------------------
            private void AlignLine(XParagraphAlignment alignment, int firstIndex, int lastIndex, double layoutWidth)
            {
                XParagraphAlignment blockAlignment = ((Block)(this.blocks[firstIndex])).Alignment;
                if (alignment == XParagraphAlignment.Left || blockAlignment == XParagraphAlignment.Left)
                    return;

                int count = lastIndex - firstIndex + 1;
                if (count == 0)
                    return;

                double totalWidth = -this.spaceWidth;
                for (int idx = firstIndex; idx <= lastIndex; idx++)
                    totalWidth += ((Block)(this.blocks[idx])).Width + this.spaceWidth;

                double dx = Math.Max(layoutWidth - totalWidth, 0);
                //Debug.Assert(dx >= 0);
                if (alignment != XParagraphAlignment.Justify)
                {
                    if (alignment == XParagraphAlignment.Center)
                        dx /= 2;
                    for (int idx = firstIndex; idx <= lastIndex; idx++)
                    {
                        Block block = (Block)this.blocks[idx];
                        block.Location += new XSize(dx, 0);
                    }
                }
                else if (count > 1) // case: justify
                {
                    dx /= count - 1;
                    for (int idx = firstIndex + 1, i = 1; idx <= lastIndex; idx++, i++)
                    {
                        Block block = (Block)this.blocks[idx];
                        block.Location += new XSize(dx * i, 0);
                    }
                }
            }
        }


        ///<summary>
        ///Classe importate da pdfsharp, file XTextFormatter.cs
        ///NON MODIFICATA: ricopiata per motivi di visibilita' (e' internal sealed )
        /// Represents a single word.
        /// </summary>
        //================================================================================
        internal sealed class Block
        {
            /// <summary>
            /// The text represented by this block.
            /// </summary>
            public string Text;

            /// <summary>
            /// The type of the block.
            /// </summary>
            public BlockType Type;

            /// <summary>
            /// The width of the text.
            /// </summary>
            public double Width;

            /// <summary>
            /// The location relative to the upper left corner of the layout rectangle.
            /// </summary>
            public XPoint Location;

            /// <summary>
            /// The alignment of this line.
            /// </summary>
            public XParagraphAlignment Alignment;

            /// <summary>
            /// A flag indicating that this is the last bock that fits in the layout rectangle.
            /// </summary>
            public bool Stop;
            /// <summary>
            /// Initializes a new instance of the <see cref="Block"/> class.
            /// </summary>
            /// <param name="text">The text of the block.</param>
            /// <param name="type">The type of the block.</param>
            /// <param name="width">The width of the text.</param>
            public Block(string text, BlockType type, double width)
            {
                Text = text;
                Type = type;
                Width = width;
            }


            /// <summary>
            /// Initializes a new instance of the <see cref="Block"/> class.
            /// </summary>
            /// <param name="type">The type.</param>
            public Block(BlockType type)
            {
                Type = type;
            }

        }
    }
}
