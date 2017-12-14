using System;
using System.Collections.Generic;
using System.IO;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using spreadsheet = DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Windows;
using System.Windows.Forms;
using System.Text;

namespace Microarea.TaskBuilderNet.UI.ReportsRenders
{
    public class OpenXmlExcelRender
    {

        private bool showTotals = false;
        private bool repeatTitles = false;
        private string filePath = string.Empty;
        private string sheetName = string.Empty;
        private int firstRow = 1;
        private int headerRow = 1;
        private int firstColumn = 1;
        private string dataFormat = string.Empty;
        private string dataTimeFormat = string.Empty;
        private string timeFormat = string.Empty;
        private bool autoSave = false;

        private string[] titles = null;
        private object[,] cellsValues = null;

        public OpenXmlExcelRender(string filePath, string sheetName, int firstRow, int firstColumn, string dataFormat,
                                    string dataTimeFormat, string timeFormat, bool repeatTitles, bool showTotals, bool autoSave)
        {
            this.filePath = filePath;
            this.sheetName = sheetName;
            this.firstRow = this.firstRow + firstRow;
            this.firstColumn = firstColumn;
            this.repeatTitles = repeatTitles;

            headerRow = firstRow;

            this.dataFormat = dataFormat;
            this.dataTimeFormat = dataTimeFormat;
            this.timeFormat = timeFormat;
            this.showTotals = showTotals;
            this.autoSave = autoSave;
        }

        public void GetExcelDataPage(object[,] cellsValues, string[] titles, object[] tot, int[] rowsForPage)
        {
            using (FileStream fs = CreateStream(filePath))
            {
                if (fs == null)
                    return;
                filePath = fs.Name.ToString();
            }

            if (string.IsNullOrEmpty(sheetName))
                sheetName = "Foglio1";

            using (SpreadsheetDocument document = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook))
            {
                //crea il documento Excel
                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                //crea il foglio 
                WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet();

                // Aggiungo lo stile
                WorkbookStylesPart stylePart = workbookPart.AddNewPart<WorkbookStylesPart>();
                stylePart.Stylesheet = GenerateStylesheet();
                stylePart.Stylesheet.Save();

                SheetData sheetData = new SheetData();

                // Costruzione dell'header
                Row row = new Row();
                this.titles = titles;

                List<Column> columList = new List<Column>();
                List<Cell> cells = new List<Cell>();
                Columns columns = new Columns();
                uint minCol = 0;
                uint maxCol = 0;
                int numCols = titles.Length;

                for (int col = 0; col < numCols; col++)
                {
                    int width = titles[col].Length + 10;
                    minCol++;
                    maxCol++;
                    columList.Add(new Column
                    {
                        Min = minCol,
                        Max = maxCol,
                        Width = width,
                        CustomWidth = true
                    });
                    cells.Add(ConstructCell(titles[col].ToString(), CellValues.String, 2));
                }
                columns.Append(columList);
                worksheetPart.Worksheet.AppendChild(columns);

                Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());
                Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = sheetName };
                sheets.Append(sheet);

                workbookPart.Workbook.Save();

                sheetData = worksheetPart.Worksheet.AppendChild(new SheetData());

                //Inserimento della riga di header del foglio
                row.Append(cells);
                sheetData.AppendChild(row);

                this.cellsValues = cellsValues;

                if (cellsValues == null)
                    return;

                int bound0 = cellsValues.GetUpperBound(0);
                int bound1 = cellsValues.GetUpperBound(1);

                int totForPage = 0;
                int pageindex = 0;

                if (titles == null || titles.Length == 0)
                    firstRow = firstRow - 1;
                for (int i = 0; i <= bound0; i++)
                {
                    row = new Row();
                    List<Cell> dataCells = new List<Cell>();
                    for (int x = 0; x <= bound1; x++)
                    {
                        dataCells.Add(ConstructCell(cellsValues[i, x].ToString(), CellValues.String, 1));
                    }
                    row.Append(dataCells);
                    sheetData.AppendChild(row);

                    totForPage = totForPage + 1;

                    if (repeatTitles && totForPage == rowsForPage[pageindex] && rowsForPage.Length > 1 && i != bound0)
                    {
                        row = new Row();
                        for (int col = 0; col < numCols; col++)
                        {
                            int width = titles[col].Length + 10;
                            minCol++;
                            maxCol++;
                            columList.Add(new Column
                            {
                                Min = minCol,
                                Max = maxCol,
                                Width = width,
                                CustomWidth = true
                            });
                            cells.Add(ConstructCell(titles[col].ToString(), CellValues.String, 2));
                        }

                        columns.Append(columList);
                        worksheetPart.Worksheet.AppendChild(columns);

                        row.Append(cells);
                        sheetData.AppendChild(row);

                        firstRow = firstRow + 1;
                        pageindex = pageindex + 1;
                        totForPage = 0;
                    }
                }
                if (showTotals)
                {
                    row = new Row();
                    List<Cell> dataTotal = new List<Cell>();
                    for (int j = 0; j < tot.Length; j++)
                    {
                        dataTotal.Add(ConstructCell(tot[j].ToString(), CellValues.String, 1));
                    }
                    row.Append(dataTotal);
                    sheetData.AppendChild(row);
                }

                if (bound0 == -1)
                    bound0 = 0;

                worksheetPart.Worksheet.Save();
            }
        }

        private Cell ConstructCell(string value, CellValues dataType, uint styleIndex = 0)
        {
            return new Cell()
            {
                CellValue = new CellValue(value),
                DataType = new EnumValue<CellValues>(dataType),
                StyleIndex = styleIndex,
            };
        }

        private Stylesheet GenerateStylesheet()
        {
            Stylesheet styleSheet = null;

            Fonts fonts = new Fonts(
                new Font( // Index 0 - default
                    new FontSize() { Val = 10 }

                ),
                new Font( // Index 1 - header
                    new FontSize() { Val = 10 },
                    new Bold(),
                    new Color() { Rgb = "FFFFFF" }

                ));

            Fills fills = new Fills(
                    new Fill(new PatternFill() { PatternType = PatternValues.None }), // Index 0 - default
                    new Fill(new PatternFill() { PatternType = PatternValues.Gray125 }), // Index 1 - default
                    new Fill(new PatternFill(new ForegroundColor { Rgb = new HexBinaryValue() { Value = "66666666" } })
                    { PatternType = PatternValues.Solid }) // Index 2 - header
                );

            Borders borders = new Borders(
                    new Border(), // index 0 default
                    new Border( // index 1 black border
                        new LeftBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                        new RightBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                        new TopBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                        new BottomBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                        new DiagonalBorder())
                );

            CellFormats cellFormats = new CellFormats(
                    new CellFormat(), // default
                    new CellFormat { FontId = 0, FillId = 0, BorderId = 1, ApplyBorder = true }, // body
                    new CellFormat { FontId = 1, FillId = 2, BorderId = 1, ApplyFill = true } // header
                );

            styleSheet = new Stylesheet(fonts, fills, borders, cellFormats);

            return styleSheet;
        }

        private int ConvertPixelToMm(int pixel)
        {
            using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero))
            {
                int dpiX = (int)graphics.DpiX;
                int mm = pixel * (int)25.4 / dpiX;
                return mm;
            }
        }

        private FileStream CreateStream(string filePath)
        {
            FileStream fs;
            if (string.IsNullOrEmpty(filePath))
            {
                SaveFileDialog open = new SaveFileDialog();
                open.Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
                open.ShowDialog();
                if (String.IsNullOrEmpty(open.FileName))
                    return null;
                else
                    fs = File.Create(open.FileName);
            }
            else
                fs = File.Create(filePath);
            return fs;
        }
    }

    public class CustomColumn : Column
    {
        public CustomColumn(UInt32 startColumnIndex,
               UInt32 endColumnIndex, double columnWidth)
        {
            this.Min = startColumnIndex;
            this.Max = endColumnIndex;
            this.Width = columnWidth;
            this.CustomWidth = true;
        }
    }
}
