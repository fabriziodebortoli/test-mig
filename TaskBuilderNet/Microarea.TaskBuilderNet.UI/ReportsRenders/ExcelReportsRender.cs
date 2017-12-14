using Microarea.TaskBuilderNet.Core.NameSolver;
using Microsoft.Office.Interop.Excel;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using Microsoft.CSharp.RuntimeBinder;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.ReportsRenders
{
	//=========================================================================
	public class ExcelReportsRender : IDisposable
	{
		private Microsoft.Office.Interop.Excel.Application app = null;
		private Workbook     workbook;
		private Worksheet    worksheet;

        private bool    showTotals = false;
        private bool    repeatTitles = false;
		private string  filePath = string.Empty;
		private string  sheetName = string.Empty;
		private int     firstRow = 1;
		private int     headerRow = 1;
		private int     firstColumn = 1;
		private string  dataFormat = string.Empty;
		private string  dataTimeFormat = string.Empty;
		private string  timeFormat = string.Empty;
        private bool    autoSave = false;

        private string[]    titles = null;
		private object[,]   cellsValues = null; 


		public string FilePath { get { return filePath; } set { filePath = value; } }
		public string SheetName { get { return sheetName; } set { sheetName = value; } }

		public int FirstRow { get { return firstRow; } set { firstRow = value; } }
		public int FirstColumn { get { return firstColumn; } set { firstColumn = value; } }

		public string   DataFormat { get { return dataFormat; } set { dataFormat = value; } }
		public string   DataTimeFormat { get { return dataTimeFormat; } set { dataTimeFormat = value; } }
		public string   TimeFormat { get { return timeFormat; } set { timeFormat = value; } }
        public bool     RepeatTitles { get { return repeatTitles; } set { repeatTitles = value; } }
        public bool     ShowTotals { get { return showTotals; } set { showTotals = value; } }

		public string[] columnsName = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W" };

		//--------------------------------------------------------------------------------------------------------------------------------
        public ExcelReportsRender(string filePath, string sheetName, int firstRow, int firstColumn, string dataFormat, 
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

		//---------------------------------------------------------------------
        public void CreateExcel(object[,] cellsValues, string[] titles, object[] tot, int[] rowsForPage)
		{
            try
            {
                app = new Microsoft.Office.Interop.Excel.Application();
            }
            catch (Exception)
            {
                MessageBox.Show(Strings.NoExcel);
                return;
            }

			app.Visible = true;

			if (!string.IsNullOrEmpty(filePath))
			{
                try
                {
					if (!File.Exists(filePath))
					{
						workbook = app.Workbooks.Add(1);
						worksheet = (Worksheet)workbook.Sheets[1];
						workbook.SaveAs(filePath, Microsoft.Office.Interop.Excel.XlFileFormat.xlExcel8,
										Type.Missing, Type.Missing, false, false,
										XlSaveAsAccessMode.xlShared, XlSaveConflictResolution.xlLocalSessionChanges,
										Type.Missing, Type.Missing);
					}
					workbook = app.Workbooks.Open(filePath, 0, false, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
				}
				catch (Exception exx)
                {
                    string a = exx.Message;
                }

                if (string.IsNullOrEmpty(sheetName))//Se nn specifico lo sheetname ne aggiungo uno nuovo di default
                    worksheet = (Worksheet)workbook.Worksheets.Add();

                if (!string.IsNullOrEmpty(sheetName))
                {
                    bool existSheet = false;
                    foreach (Worksheet sheet in workbook.Worksheets)
                    {
                        if (sheet.Name == sheetName)
                        {
                            worksheet = sheet;
                            existSheet = true;
                        }
                    }
                    if (!existSheet)
                    {
                        worksheet = (Worksheet)workbook.Worksheets.Add();
                        worksheet.Name = sheetName;
                    }
                 }
			}
			else
			{
                workbook = app.Workbooks.Add(1);
                worksheet = (Worksheet)workbook.Sheets[1];
            }

			if (!string.IsNullOrEmpty(sheetName))
				worksheet.Name = sheetName;

			this.titles = titles;

			for (int i = 0; i < titles.Length; i++)
				createHeader(headerRow, i + firstColumn, titles[i],  0, true, 10, string.Empty);

			this.cellsValues = cellsValues;

			if (cellsValues == null)
				return;

			int bound0 = cellsValues.GetUpperBound(0);
			int bound1 = cellsValues.GetUpperBound(1);

            int totForPage = 0;
            int pageindex  = 0;

            if (titles == null || titles.Length == 0)
                firstRow = firstRow - 1;
            for (int i = 0; i <= bound0; i++)
			{
                for (int x = 0; x <= bound1; x++)
                    addData(i + firstRow, x + this.firstColumn, cellsValues[i, x]);

                totForPage = totForPage + 1;

                if (repeatTitles && totForPage == rowsForPage[pageindex] && rowsForPage.Length > 1 && i != bound0)
                {
                    for (int ii = 0; ii < titles.Length; ii++)
                        createHeader(firstRow + i + 1, ii + firstColumn, titles[ii],0, true, 10, string.Empty);

                    firstRow = firstRow + 1;
                    pageindex = pageindex + 1;
                    totForPage = 0;
                }
            }

            if (showTotals)
            {
                for (int i = 0; i < tot.Length; i++)
                    addData(firstRow + bound0 + 1, i + this.firstColumn, tot[i]);
            }

            if (bound0 == -1)
                bound0 = 0;

        //     worksheet.get_Range("A1", "A" + "A" + (this.firstColumn + bound0).ToString()).Borders.Color = System.Drawing.Color.Black.ToArgb();
            worksheet.get_Range("A1", "A" + "A" + (this.firstColumn + bound0).ToString()).Columns.AutoFit();
            if (autoSave)
            {
                workbook.SaveAs(filePath);
                app.Quit();
            }
        }

        //---------------------------------------------------------------------
        public void PrintCorrentWorkBook(int numberOfCopy, string aPrinterName)
        {
            try
            {
                object printerName = Type.Missing;

                if (aPrinterName != string.Empty)
                    printerName = aPrinterName;

                workbook.PrintOutEx(Type.Missing, Type.Missing, numberOfCopy, Type.Missing, printerName, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
            }
            catch (Exception c)
            {
                string a = c.Message;
            }
        }
        //---------------------------------------------------------------------
        private void addData(int row, int col, object data)
        {
           // workSheet_range = worksheet.get_Range(worksheet.Cells[row, col], worksheet.Cells[row, col]);

            if (data is string)
            {
                worksheet.Cells[row, col].NumberFormat = "@";
                worksheet.Cells[row, col] = data;
                worksheet.Cells[row, col].VerticalAlignment = System.Drawing.ContentAlignment.TopLeft;
            }

            if (data is bool)
            {
                worksheet.Cells[row, col] = (bool)data;
                worksheet.Cells[row, col].VerticalAlignment = System.Drawing.ContentAlignment.TopLeft;
            }

            if (data is DateTime)
            {
                
          //      workSheet_range.NumberFormat = dataFormat;

                if (((DateTime)data).ToShortDateString() == new DateTime(1799, 12, 31).ToShortDateString())
                    worksheet.Cells[row, col].Cells[row, col] = string.Empty;
                else
                    worksheet.Cells[row, col] = ((DateTime)data);
            }

            if (data is int)
                worksheet.Cells[row, col] = (int)data;

            if (data is long)
                worksheet.Cells[row, col] = (long)data;


            if (data is double)
            {
                worksheet.Cells[row, col].HorizontalAlignment = System.Drawing.ContentAlignment.TopRight;
                worksheet.Cells[row, col] = (double)data;
                
            }

            if (data is short)
                worksheet.Cells[row, col] = (short)data;

            //worksheet.Cells[row, col] = data;
            //workSheet_range.VerticalAlignment = System.Drawing.ContentAlignment.TopLeft;

            worksheet.Cells[row, col].Borders.Color = System.Drawing.Color.Black.ToArgb();
            //worksheet.Cells[row, col].Columns.AutoFit();
        }

		//---------------------------------------------------------------------
		public void createHeader(int row, int col, string htext, int mergeColumns, bool isBold, int size, string fcolor)
		{
			worksheet.Cells[row, col] = htext;
			worksheet.Columns.AutoFit();

            worksheet.Cells[row, col].Interior.Color = System.Drawing.Color.Gainsboro.ToArgb();
            worksheet.Cells[row, col].Borders.Color = System.Drawing.Color.Black.ToArgb();
            worksheet.Cells[row, col].Font.Bold = isBold;
            worksheet.Cells[row, col].ColumnWidth = size;

			if (fcolor.Equals(""))
                worksheet.Cells[row, col].Font.Color = System.Drawing.Color.White.ToArgb();
			else
                worksheet.Cells[row, col].Font.Color = System.Drawing.Color.Black.ToArgb();
		}

		//Verifica se fare così o in altro modo LARA
		//---------------------------------------------------------------------
		public void Dispose()
		{
			System.Runtime.InteropServices.Marshal.FinalReleaseComObject(app);
			System.Runtime.InteropServices.Marshal.FinalReleaseComObject(workbook);
			System.Runtime.InteropServices.Marshal.FinalReleaseComObject(worksheet);
		}
	}


    //=========================================================================
    public class ExcelBasileaReportsRender : IDisposable
    {
        private Microsoft.Office.Interop.Excel.Application app = null;
        private Workbook workbook;
        private Worksheet worksheet;
        private Worksheet worksheet2;
        private Worksheet worksheet3;

        private const string excelPathToken = "Excel";
        private const string itPathToken = "it-it";
        private const string templateFileName = "Basilea2.xlsx";
        private const string applicationPathToken = "ERP";
        private const string modulePathToken = "Basel_II";
        private const string wSheet1Name = "Bilancio";
        private const string wSheet2Name = "Riclassificazione";
        private const string wSheet3Name = "Messaggi";

        private string filePath = string.Empty;
        private string dataFormat = string.Empty;
        private string dataTimeFormat = string.Empty;
        private string timeFormat = string.Empty;

       private object[,] cellsValues = null;


        public string FilePath { get { return filePath; } set { filePath = value; } }
        public string DataFormat { get { return dataFormat; } set { dataFormat = value; } }
        public string DataTimeFormat { get { return dataTimeFormat; } set { dataTimeFormat = value; } }
        public string TimeFormat { get { return timeFormat; } set { timeFormat = value; } }


        public string[] columnsName = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W" };

        //--------------------------------------------------------------------------------------------------------------------------------
        public ExcelBasileaReportsRender(string filePath,string dataFormat, string dataTimeFormat, string timeFormat)
        {
            this.filePath = filePath;
            this.dataFormat = dataFormat;
            this.dataTimeFormat = dataTimeFormat;
            this.timeFormat = timeFormat;
           
        }

        //---------------------------------------------------------------------
        public void CreateExcel(object[,] cellsValues)
        {
            app = new Microsoft.Office.Interop.Excel.Application();
            app.WindowState = XlWindowState.xlMinimized;


            string path = BasePathFinder.BasePathFinderInstance.GetApplicationModulePath(applicationPathToken, modulePathToken);
            path = Path.Combine(path, excelPathToken);
            path = Path.Combine(path, itPathToken);
            path = Path.Combine(path, templateFileName);

            workbook = app.Workbooks.Open(path, 0, false, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
            //TODO LARA
            foreach (Worksheet sheet in workbook.Worksheets)
            {
                if (sheet.Name == wSheet1Name)
                {
                    worksheet = sheet;
                    worksheet.Select();
                }

                if (sheet.Name == wSheet2Name)
                    worksheet2 = sheet;

                if (sheet.Name == wSheet3Name)
                    worksheet3 = sheet;
            }

            this.cellsValues = cellsValues;

            if (cellsValues == null)
                return;

            int bound0 = cellsValues.GetUpperBound(0);

            for (int i = 0; i <= bound0; i++)
            {
                addData(cellsValues[i, 0], cellsValues[i, 1]);
            }

            workbook.SaveAs(filePath);
        }

        //---------------------------------------------------------------------
        private void ReplaceValue(Range range, object data, object field)
        {
            if (data is double)
            {
                range.NumberFormat = "0.00";
                range.Value = (double)data;
                return;
            }

            if (data is DateTime)
            {
                range.Value = (DateTime)data;
                return;
            }

            if (data is string)
            {
                range.Value = (string)data;
                return;
            }

            range.Replace(field.ToString(), data);

            return;
        
        }

        //---------------------------------------------------------------------
        public void addData(object data, object field)
        {

            Range range = worksheet.Cells.Find(field.ToString(), Missing.Value, Missing.Value, XlLookAt.xlWhole);
            if (range != null)
            {
               ReplaceValue(range, data, field);
               return;
            }

            range = worksheet2.Cells.Find(field.ToString(), Missing.Value, XlFindLookIn.xlValues);
            if (range != null)
            {
               ReplaceValue(range, data, field);
               return;
            }

            range = worksheet3.Cells.Find(field.ToString(), Missing.Value, XlFindLookIn.xlValues);
            if (range != null)
            {
                ReplaceValue(range, data, field);
                return;
            }
        }

        //Verifica se fare così o in altro modo LARA
        //---------------------------------------------------------------------
        public void Dispose()
        {
            Marshal.FinalReleaseComObject(app);
            Marshal.FinalReleaseComObject(workbook);
            Marshal.FinalReleaseComObject(worksheet);
        }
    }

    //=========================================================================
    public class ExcelTemplateReportsRender : IDisposable
    {
        private Microsoft.Office.Interop.Excel.Application app = null;
        private Workbook    workbook;

        private const string excelPathToken         = "Excel";
        private const string itPathToken            = "it-it";
        private const string templateFileName       = "Basilea2_Smart.xlsx";
        private const string applicationPathToken   = "ERP";
        private const string modulePathToken        = "Basel_II";

        private string filePath     = string.Empty;
        private string templateFile = string.Empty;
      
        private object[,] cellsValues = null;


        public string FilePath { get { return filePath; } set { filePath = value; } }
        public string TemplateFile { get { return templateFile; } set { templateFile = value; } }

      
        //--------------------------------------------------------------------------------------------------------------------------------
        public ExcelTemplateReportsRender(string filePath, string templateFile)
        {
            this.filePath = filePath;
            this.templateFile = templateFile;
        }

        //---------------------------------------------------------------------
        public void CreateExcel(object[,] cellsValues)
        {
            app = new Microsoft.Office.Interop.Excel.Application();
            app.Visible = true;

            string path = templateFile;
            if (string.IsNullOrEmpty(path))
            { 
                path = BasePathFinder.BasePathFinderInstance.GetApplicationModulePath(applicationPathToken, modulePathToken);
                path = Path.Combine(path, excelPathToken);
                path = Path.Combine(path, itPathToken);
                path = Path.Combine(path, templateFileName);
            }

            workbook = app.Workbooks.Open(path, 0, false, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);

            this.cellsValues = cellsValues;

            if (cellsValues == null)
                return;

            int bound0 = cellsValues.GetUpperBound(0);

            for (int i = 0; i <= bound0; i++)
            {
                addData(cellsValues[i, 0], cellsValues[i, 1]);
            }

            if (!string.IsNullOrEmpty(filePath))
                workbook.SaveAs(filePath);
        }

        //---------------------------------------------------------------------
        public void addData(object data, object field)
        {
            string sheetName = field.ToString().Substring(0, field.ToString().IndexOf('.'));

            Range cellReange = null;

            foreach (Worksheet sheet in workbook.Worksheets)
            {

                if (sheetName == sheet.Name)
                {
                    string cell = field.ToString().Substring(field.ToString().IndexOf('.') + 1);
                    cellReange = sheet.get_Range(cell);

                    if (cellReange != null)
                    {
                        cellReange.set_Value(Missing.Value, data);
                        return;
                    }
                }
            }
        }

        //Verifica se fare così o in altro modo LARA
        //---------------------------------------------------------------------
        public void Dispose()
        {
            Marshal.FinalReleaseComObject(app);
            Marshal.FinalReleaseComObject(workbook);
        }
    }

}
