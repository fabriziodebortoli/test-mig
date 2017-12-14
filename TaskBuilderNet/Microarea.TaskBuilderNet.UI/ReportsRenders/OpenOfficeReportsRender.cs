using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using uno;
using unoidl.com.sun.star.awt;
using unoidl.com.sun.star.beans;
using unoidl.com.sun.star.container;
using unoidl.com.sun.star.frame;
using unoidl.com.sun.star.lang;
using unoidl.com.sun.star.sheet;
using unoidl.com.sun.star.table;
using unoidl.com.sun.star.text;
using unoidl.com.sun.star.uno;

namespace Microarea.TaskBuilderNet.UI.ReportsRenders
{
    //=========================================================================
    public class OpenOfficeSheetRender : IDisposable
    {
        private XSpreadsheet oSheet = null;
        private string filePath = string.Empty;
        private string sheetName = string.Empty;
        private int firstRow = -1;
        private int headerRow = -2;
        private int firstColumn = -1;
        private string dataFormat = string.Empty;
        private string dataTimeFormat = string.Empty;
        private string timeFormat = string.Empty;
        private bool showTotals = false;
        private bool repeatTitles = false;
        private string[] titles = null;
        private object[,] cellsValues = null;

        public string FilePath { get { return filePath; } set { filePath = value; } }
        public string SheetName { get { return sheetName; } set { sheetName = value; } }

        public int FirstRow { get { return firstRow; } set { firstRow = value; } }
        public int FirstColumn { get { return firstColumn; } set { firstColumn = value; } }

        public string DataFormat { get { return dataFormat; } set { dataFormat = value; } }
        public string DataTimeFormat { get { return dataTimeFormat; } set { dataTimeFormat = value; } }
        public string TimeFormat { get { return timeFormat; } set { timeFormat = value; } }
        public bool RepeatTitles { get { return repeatTitles; } set { repeatTitles = value; } }
        public bool ShowTotals { get { return showTotals; } set { showTotals = value; } }

        //--------------------------------------------------------------------------------------------------------------------------------
        public OpenOfficeSheetRender()
        {

        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public OpenOfficeSheetRender(string filePath, string sheetName, int firstRow, int firstColumn, string dataFormat,
                                        string dataTimeFormat, string timeFormat, bool repeatTitles, bool showTotals)
        {
            this.filePath = filePath;
            this.sheetName = sheetName;
            this.firstRow = this.firstRow + firstRow;
            this.firstColumn = this.firstColumn + firstColumn;
            this.repeatTitles = repeatTitles;

            headerRow = this.firstRow;

            this.dataFormat = dataFormat;
            this.dataTimeFormat = dataTimeFormat;
            this.timeFormat = timeFormat;
            this.showTotals = showTotals;
        }

        //---------------------------------------------------------------------
        public void CreateODS(object[,] cellsValues, string[] titles, object[] tot, int[] rowsForPage)
        {
            this.cellsValues = cellsValues;
            this.titles = titles;
            XComponentContext oStrap = null;
            try
            {
                oStrap = uno.util.Bootstrap.bootstrap();
            }
            catch (System.Exception) //SEHException exception)
            {
                MessageBox.Show(Strings.NoOpenOffice);
                return;
            }

            XComponent oDoc;
            XMultiServiceFactory oServMan = (XMultiServiceFactory)oStrap.getServiceManager();
            XComponentLoader oDesk = (XComponentLoader)oServMan.createInstance("com.sun.star.frame.Desktop");

            if (string.IsNullOrEmpty(filePath))
            {
                string url = @"private:factory/scalc";
                PropertyValue[] propVals = new PropertyValue[0];
                oDoc = oDesk.loadComponentFromURL(url, "_blank", 0, propVals);
            }
            else
            {
                PropertyValue[] loadProps = new PropertyValue[1];
                loadProps[0] = new PropertyValue();
                loadProps[0].Name = "AsTemplate";
                loadProps[0].Value = new uno.Any(false);
                oDoc = oDesk.loadComponentFromURL("file:///" + filePath.Replace(@"\", "/"), "_blank", 0, loadProps);
            }

            XSpreadsheets oSheets = ((XSpreadsheetDocument)oDoc).getSheets();
            XIndexAccess oSheetsIA = (XIndexAccess)oSheets;
            oSheet = (XSpreadsheet)oSheetsIA.getByIndex(0).Value;
            //XPrintAreas print ;
            //print = (XPrintAreas) oSheets;
            //CellRangeAddress[] range = print.getPrintAreas();
            //oDoc.Print(null);


            for (int i = 0; i < titles.Length; i++)
                addHeaderData(headerRow, i + firstColumn, titles[i]);

            this.cellsValues = cellsValues;

            if (cellsValues == null)
                return;

            int bound0 = cellsValues.GetUpperBound(0);
            int bound1 = cellsValues.GetUpperBound(1);

            int totForPage = 0;
            int pageindex = 0;

            for (int i = 0; i <= bound0; i++)
            {
                for (int x = 0; x <= bound1; x++)
                    addData(i + this.firstRow + 1, x + firstColumn, cellsValues[i, x]);

                totForPage = totForPage + 1;

                if (repeatTitles && totForPage == rowsForPage[pageindex] && rowsForPage.Length > 1 && i != bound0)
                {
                    for (int ii = 0; ii < titles.Length; ii++)
                        addHeaderData(firstRow + i + 1, ii + firstColumn, titles[ii]);
                    firstRow = firstRow + 1;
                    pageindex = pageindex + 1;
                    totForPage = 0;
                }
            }

            if (showTotals)
            {
                for (int i = 0; i < tot.Length; i++)
                    addData(firstRow + bound0 + 2, i + this.firstColumn, tot[i]);
            }

            XCellRange Range = null;
            for (int i = 0; i <= bound1; i++)
            {
                Range = oSheet.getCellRangeByName(ExcelColumnFromNumber(i + 1 + firstColumn) + "1"); //Recover the range, a cell is 

                XColumnRowRange RCol = (XColumnRowRange)Range; //Creates a collar ranks 
                XTableColumns LCol = RCol.getColumns(); // Retrieves the list of passes
                uno.Any Col = LCol.getByIndex(0); //Extract the first Col

                XPropertySet xPropSet = (XPropertySet)Col.Value;
                xPropSet.setPropertyValue("OptimalWidth", new Any((bool)true));
                xPropSet.setPropertyValue("VertJustify", new Any((int)unoidl.com.sun.star.table.CellVertJustify.TOP));
            }
        }

        //---------------------------------------------------------------------
        public static string ExcelColumnFromNumber(int column)
        {
            string columnString = "";
            decimal columnNumber = column;
            while (columnNumber > 0)
            {
                decimal currentLetterNumber = (columnNumber - 1) % 26;
                char currentLetter = (char)(currentLetterNumber + 65);
                columnString = currentLetter + columnString;
                columnNumber = (columnNumber - (currentLetterNumber + 1)) / 26;
            }
            return columnString;
        }

        //---------------------------------------------------------------------
        public bool IsEnabledOpenOffice()
        {
            XComponentContext oStrap = null;

            try
            {
                oStrap = uno.util.Bootstrap.bootstrap();
                return true;
            }
            catch (SEHException)
            {
                //  MessageBox.Show(exception.Message);
                return false;
            }
            catch (unoidl.com.sun.star.uno.Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
            catch (System.Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }

        //---------------------------------------------------------------------
        public void addData(int row, int col, object data)
        {
            XCell oCell = oSheet.getCellByPosition(col, row); //A1
            if (data is string)
            {
                ((XText)oCell).setString((string)data);
            }
            else if (data is Int32)
            {
                oCell.setValue(Convert.ToInt32(data));
            }
            else if (data is Int16)
            {
                oCell.setValue(Convert.ToInt16(data));
            }
            else if (data is Int64)
            {
                oCell.setValue(Convert.ToInt64(data));
            }
            else if (data is double)
            {
                oCell.setValue(Convert.ToDouble(data));
            }
            else if (data is DateTime)
            {
                ((XText)oCell).setString(((DateTime)data).ToShortDateString());
            }
            else if (data is Boolean)
            {
                ((XText)oCell).setString(data.ToString());
            }
            else
            {
                try { ((XText)oCell).setString(data.ToString()); } catch (System.Exception ex) { Debug.Fail(ex.Message); }
            }
        }

        //---------------------------------------------------------------------
        public void addHeaderData(int row, int col, object data)
        {
            XCell oCell = null;

            if (data is Int32)
            {
                oCell = oSheet.getCellByPosition(col, row);
                oCell.setValue(Convert.ToInt32(data));
                ((XPropertySet)oCell).setPropertyValue("CellBackColor", new uno.Any((int)0xC0C0C0));
                ((XPropertySet)oCell).setPropertyValue("CharWeight", new uno.Any((Single)FontWeight.BOLD));
                ((XPropertySet)oCell).setPropertyValue("CellStyle", new uno.Any((string)"Heading"));
            }

            if (data is string)
            {
                oCell = oSheet.getCellByPosition(col, row); //A1
                ((XText)oCell).setString((string)data);
                ((XPropertySet)oCell).setPropertyValue("CellBackColor", new uno.Any((int)0xC0C0C0));
                ((XPropertySet)oCell).setPropertyValue("CharWeight", new uno.Any((Single)FontWeight.BOLD));
                ((XPropertySet)oCell).setPropertyValue("CellStyle", new uno.Any((string)"Heading"));

            }

        }

        public void Dispose()
        {
        }
   }

    //=========================================================================
    public class OpenOfficeWriterRender : IDisposable
    {
        private string filePath = string.Empty;
        private string sheetName = string.Empty;
        private int firstRow = 0;
        private int headerRow = -1;
        private int firstColumn = -1;
        private string dataFormat = string.Empty;
        private string dataTimeFormat = string.Empty;
        private string timeFormat = string.Empty;
        private bool showTotals = false;
        private bool repeatTitles = false;
        private XComponent oDoc;


            private string[] titles = null;
            private object[,] cellsValues = null;

            public string FilePath { get { return filePath; } set { filePath = value; } }
            public string SheetName { get { return sheetName; } set { sheetName = value; } }

            public int FirstRow { get { return firstRow; } set { firstRow = value; } }
            public int FirstColumn { get { return firstColumn; } set { firstColumn = value; } }

            public string DataFormat { get { return dataFormat; } set { dataFormat = value; } }
            public string DataTimeFormat { get { return dataTimeFormat; } set { dataTimeFormat = value; } }
            public string TimeFormat { get { return timeFormat; } set { timeFormat = value; } }
            public bool RepeatTitles { get { return repeatTitles; } set { repeatTitles = value; } }
            public bool ShowTotals { get { return showTotals; } set { showTotals = value; } }

        //--------------------------------------------------------------------------------------------------------------------------------
        public OpenOfficeWriterRender(string filePath, string sheetName, int firstRow, int firstColumn, string dataFormat,
                                        string dataTimeFormat, string timeFormat, bool repeatTitles, bool showTotals)
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
            }

            //---------------------------------------------------------------------
            public void CreateODT(object[,] cellsValues, string[] titles, object[] tot, int[] rowsForPage)
            {
                this.cellsValues = cellsValues;
                this.titles = titles;
                XComponentContext oStrap = null;
                try
                {
                    oStrap = uno.util.Bootstrap.bootstrap();
                }
                catch (System.Exception)//SEHException exception
                {
                    MessageBox.Show(Strings.NoOpenOffice);
                    return;
                }

            XMultiServiceFactory oServMan = (XMultiServiceFactory)oStrap.getServiceManager();
            XComponentLoader oDesk = (XComponentLoader)oServMan.createInstance("com.sun.star.frame.Desktop");

            if (string.IsNullOrEmpty(filePath))
            {
                string url = @"private:factory/swriter";
                PropertyValue[] propVals = new PropertyValue[0];
                oDoc = oDesk.loadComponentFromURL(url, "_blank", 0, propVals);
            }
            else
            {
                PropertyValue[] loadProps = new PropertyValue[1];
                loadProps[0] = new PropertyValue();
                loadProps[0].Name = "AsTemplate";
                loadProps[0].Value = new uno.Any(false);
                oDoc = oDesk.loadComponentFromURL("file:///" + filePath.Replace(@"\", "/"), "_blank", 0, loadProps);
            }

                XText text = ((XTextDocument)oDoc).getText();
                XTextCursor cursor = text.createTextCursor();

                int bound0 = cellsValues.GetUpperBound(0);
                int bound1 = cellsValues.GetUpperBound(1);

            XTextTable table = (XTextTable)((XMultiServiceFactory)oDoc).createInstance("com.sun.star.text.TextTable");
            if (ShowTotals)
                table.initialize(bound0 + 3, bound1 + 1);
            else
                table.initialize(bound0 + 2, bound1 + 1);

                text.insertTextContent(cursor, table, false);

                for (int i = 0; i < titles.Length; i++)
                    InsertHeadersIntoCell(OpenOfficeSheetRender.ExcelColumnFromNumber(i + 1) + "1", titles[i], table);

                this.cellsValues = cellsValues;

                if (cellsValues == null)
                    return;

                int totForPage = 0;
                int pageindex = 0;

            for (int i = 0; i <= bound0; i++)
            {
                for (int x = 0; x <= bound1; x++)
                {
                    string cellText = string.Empty;

                    if (cellsValues[i, x] is DateTime)
                        cellText = ((DateTime)cellsValues[i, x]).ToShortDateString();
                    else cellText = cellsValues[i, x].ToString();

                    InsertHeadersIntoCell(OpenOfficeSheetRender.ExcelColumnFromNumber(x + firstColumn) + (i + this.firstRow + 1).ToString(), cellText, table);
                }

                    totForPage = totForPage + 1;

                if (repeatTitles && totForPage == rowsForPage[pageindex] && rowsForPage.Length > 1 && i != bound0)
                {
                    for (int ii = 0; ii < titles.Length; ii++)
                        InsertHeadersIntoCell(OpenOfficeSheetRender.ExcelColumnFromNumber(ii + firstColumn) + (firstRow + i + 2).ToString(), titles[ii].ToString(), table);
                    firstRow = firstRow + 1;
                    pageindex = pageindex + 1;
                    totForPage = 0;
                }
            }

                if (showTotals)
                {
                    for (int i = 0; i < tot.Length; i++)
                        InsertHeadersIntoCell(OpenOfficeSheetRender.ExcelColumnFromNumber(i + this.firstColumn) + (firstRow + bound0 + 2).ToString(), tot[i].ToString(), table);
                }

            }

            ////---------------------------------------------------------------------
            //public void PrintCorrentDocument(int numberOfCopy, string aPrinterName)
            //{
            //    try
            //    {
            //        object printerName = Type.Missing;

            //        if (aPrinterName != string.Empty)
            //            printerName = aPrinterName;

            //        ((XTextDocument)oDoc).p
            //    }
            //    catch (Exception c)
            //    {
            //        string a = c.Message;
            //    }

        //}
        //---------------------------------------------------------------------
        public static void InsertHeadersIntoCell(String sCellName, String sText, XTextTable xTable)
        {
            XText xCellText = (XText)xTable.getCellByName(sCellName);
            if (xCellText != null)
                xCellText.setString(sText);

                ////BACKGROUND COLORS:
                //// Select the table headers and get the cell properties:
                //XCellRange xCellRange = ( XCellRange )UnoRuntime.queryInterface( XCellRange.class, xTable );
                //XCellRange xSelectedCells = xCellRange.getCellRangeByName("A1:C1");
                //XPropertySet xCellProps = (XPropertySet)UnoRuntime.queryInterface(XPropertySet.class, xSelectedCells);
                //// Format the color of the table headers (page 56 and 57):
                //xTableProps = (XPropertySet)UnoRuntime.queryInterface(XPropertySet.class, xCellRange);
                //xCellProps.setPropertyValue("BackColor", new Integer(0x000052));

                //// BORDERS:
                //// Define a border line, then assign a color and a width:
                //BorderLine theLine = new BorderLine();
                //theLine.Color = 0x000099;
                //theLine.OuterLineWidth = 1;
                //// Apply the line definition to all cell borders and make them valid:
                //TableBorder bord = new TableBorder();
                //bord.VerticalLine = bord.HorizontalLine =
                //        bord.LeftLine = bord.RightLine =
                //        bord.TopLine = bord.BottomLine =
                //        theLine;
                //bord.IsVerticalLineValid = bord.IsHorizontalLineValid =
                //        bord.IsLeftLineValid = bord.IsRightLineValid =
                //        bord.IsTopLineValid = bord.IsBottomLineValid =
                //        true;
                //xTableProps.setPropertyValue("TableBorder", bord);

            }
            //---------------------------------------------------------------------
            public void Dispose()
            {
            }
        }
}