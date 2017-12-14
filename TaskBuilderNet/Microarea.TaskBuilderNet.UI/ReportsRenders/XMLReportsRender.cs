using System;
using System.Xml;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Windows.Forms;
using System.Web.UI;
using System.Text;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TBPicComponents;

namespace Microarea.TaskBuilderNet.UI.ReportsRenders
{
	//=========================================================================
	public class XMLReportsRender : IDisposable
	{
        private XmlDocument doc;
        private XmlElement  reportDataNode;
        private XmlElement parameters;
        private XmlElement section;
        private XmlElement group;

        private string tableName = string.Empty;
		private string filePath = string.Empty;
		private string dataFormat = string.Empty;
		private string dataTimeFormat = string.Empty;
		private string timeFormat = string.Empty;

        private string[] columsName = null;
		private object[,] cellsValues = null;
        const string nameSpaceUri = @"http://schemas.microsoft.com/office/word/2003/wordml";

        public string FilePath { get { return filePath; } set { filePath = value; } }
        public string TableName { get { return tableName; } set { tableName = value; } }
		public string DataFormat { get { return dataFormat; } set { dataFormat = value; } }
		public string DataTimeFormat { get { return dataTimeFormat; } set { dataTimeFormat = value; } }
		public string TimeFormat { get { return timeFormat; } set { timeFormat = value; } }


		//--------------------------------------------------------------------------------------------------------------------------------
		public XMLReportsRender(string filePath, string dataFormat, string dataTimeFormat, string timeFormat, string tableName)
		{
			this.filePath = filePath;
            this.tableName = tableName;
			this.dataFormat = dataFormat;
			this.dataTimeFormat = dataTimeFormat;
			this.timeFormat = timeFormat;
		}

        //--------------------------------------------------------------------------------------------------------------------------------
        public XMLReportsRender()
        {

        }

        
        //---------------------------------------------------------------------
        public string CreateXMLFull(string aReportNameSpace)
        {
            
            string reportNameSpace = aReportNameSpace.Substring(0, aReportNameSpace.LastIndexOf('.'));
            string reportName = reportNameSpace.Substring(reportNameSpace.LastIndexOf('.') +1 );

            doc = new XmlDocument();
            XmlDeclaration declaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);

            XmlElement reportNode = doc.CreateElement(XmlWriterTokens.Prefix, reportName, nameSpaceUri);
            reportNode.SetAttribute(XmlWriterTokens.Attribute.TbNamespace, reportNameSpace);

            doc.AppendChild(reportNode);

            reportDataNode = doc.CreateElement(XmlWriterTokens.Prefix, XmlWriterTokens.Element.ReportData, nameSpaceUri);
            reportNode.AppendChild(reportDataNode);

            return doc.InnerXml;
        }


        //---------------------------------------------------------------------
        public string StartParametersSection()
        {
            parameters = doc.CreateElement(XmlWriterTokens.Prefix, XmlWriterTokens.Element.Parameters, nameSpaceUri);
            reportDataNode.AppendChild(parameters);
            return doc.InnerXml;

        }

        //---------------------------------------------------------------------
        public string WriteEntryTag(string entryName, string entryTitle, string entryType, string entryLength, string entryControlType, string entryValue)
        {
            XmlElement entryNode = doc.CreateElement(XmlWriterTokens.Prefix, entryName, nameSpaceUri);
            entryNode.SetAttribute(XmlWriterTokens.Attribute.RowType, entryType);
            entryNode.SetAttribute(XmlWriterTokens.Attribute.EntryLength, entryLength);
            entryNode.SetAttribute(XmlWriterTokens.Attribute.GroupTitle, entryTitle);
            entryNode.SetAttribute(XmlWriterTokens.Attribute.ControlType, entryControlType);

            entryNode.InnerText = entryValue;

            group.AppendChild(entryNode);

            return doc.InnerXml;
        }
        //---------------------------------------------------------------------
        public string WriteGroupTag(string name, string title)
        {
            group  = doc.CreateElement(XmlWriterTokens.Prefix, name, nameSpaceUri);
            group.SetAttribute(XmlWriterTokens.Attribute.GroupTitle, title);
            //XmlAttribute attribute = doc.CreateAttribute(XmlWriterTokens.Attribute.GroupTitle, nameSpaceUri);
            //attribute.Value = title;

            //group.Attributes.Append(attribute);
            section.AppendChild(group);
            return doc.InnerXml;
        }

        //---------------------------------------------------------------------
        public string WriteAskDialogTag(string name, string title)
        {
            section =  doc.CreateElement(XmlWriterTokens.Prefix, name, nameSpaceUri);
            section.SetAttribute(XmlWriterTokens.Attribute.GroupTitle, title);
            //XmlAttribute attribute = doc.CreateAttribute(XmlWriterTokens.Attribute.GroupTitle, nameSpaceUri);
            //attribute.Value = title;

            //section.Attributes.Append(attribute);
            parameters.AppendChild(section);
            return doc.InnerXml;
        }

        //---------------------------------------------------------------------
        public string CreateSingleCells(object[,] cellsValues)
        {

            XmlElement columnNode;
            string cellValue;

            int nRows = cellsValues.GetLength(0);
            int nCols = cellsValues.GetLength(1);

            for (int r = 0; r < nRows; r++)
            {
                //for (int c = 0; c < nCols; c++)
                //{

                if (cellsValues[r, 0] != null)
                    cellValue = Convert.ToString(cellsValues[r, 0]);
                else
                    cellValue = string.Empty;

                columnNode = doc.CreateElement(XmlWriterTokens.Prefix, Convert.ToString(cellsValues[r, 1]), nameSpaceUri);
                columnNode.InnerText = cellValue;
                reportDataNode.AppendChild(columnNode);

                //   }
            }

            return doc.InnerXml;
        }

        //---------------------------------------------------------------------
        public string CreateTable(object[,] cellsValues, string[] columsName, string tableName)
        {
            XmlElement tableNode = doc.CreateElement(XmlWriterTokens.Prefix, tableName, nameSpaceUri);
            reportDataNode.AppendChild(tableNode);

            XmlElement rowNode;
            XmlElement columnNode;
            string cellValue;

            int nRows = cellsValues.GetLength(0);
            int nCols = cellsValues.GetLength(1);

            for (int r = 0; r < nRows; r++)
            {
                rowNode = doc.CreateElement(XmlWriterTokens.Prefix, XmlWriterTokens.Element.Row, nameSpaceUri);
                tableNode.AppendChild(rowNode);

                for (int c = 0; c < nCols; c++)
                {

                    if (cellsValues[r, c] != null)
                        cellValue = Convert.ToString(cellsValues[r, c]);
                    else
                        cellValue = string.Empty;

                    columnNode = doc.CreateElement(XmlWriterTokens.Prefix, columsName[c], nameSpaceUri);
                    columnNode.InnerText = cellValue;
                    rowNode.AppendChild(columnNode);

                }
            }

            return doc.InnerXml;
        }

        //---------------------------------------------------------------------
        public void SaveFullXMLL(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                SaveFileDialog open = new SaveFileDialog();
                open.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
                open.ShowDialog();
                if (String.IsNullOrEmpty(open.FileName))
                    return;
                else
                    doc.Save(open.FileName);
            }
            else
                doc.Save(filePath);
        }


        //---------------------------------------------------------------------
        public void CreateXML(object[,] cellsValues, string[] columsName)
		{
            this.columsName = columsName;

			XmlDocument doc = new XmlDocument();
            XmlDeclaration declaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);

            XmlElement dataTablesElement = doc.CreateElement("", "DataTables", "");
            doc.AppendChild(dataTablesElement);
						
			this.cellsValues = cellsValues;

			if (cellsValues == null)
				return;

			int bound0 = cellsValues.GetUpperBound(0);
			int bound1 = cellsValues.GetUpperBound(1);

            string tableName = this.tableName;
                if(string.IsNullOrEmpty(tableName))
                   tableName= "Table";

			for (int i = 0; i <= bound0; i++)
			{
                XmlElement table = doc.CreateElement("", tableName, "");
                dataTablesElement.AppendChild(table);

				for (int x = 0; x <= bound1; x++)
                    table.SetAttribute(columsName[x],  GetFormattedValue(cellsValues[i, x])); // cellsValues[i, x].ToString()); //
			}

            if (string.IsNullOrEmpty(filePath))
            {
               SaveFileDialog open = new SaveFileDialog();
               open.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
               open.ShowDialog();
               if (String.IsNullOrEmpty(open.FileName))
                    return;
                else
                    doc.Save(open.FileName);
            }
            else
                doc.Save(filePath);
		}

        //---------------------------------------------------------------------
        private string GetFormattedValue(object cellValue)
        {
            return (string)cellValue;
        }

		//Verifica se fare così o in altro modo LARA
		//---------------------------------------------------------------------
		public void Dispose()
		{

		}
	}

    //=========================================================================
    public class HTMLReportsRender : IDisposable
    {

        private string filePath = string.Empty;
        private string dataFormat = string.Empty;
        private string dataTimeFormat = string.Empty;
        private string timeFormat = string.Empty;

        private string[] titles = null;
        private object[,] cellsValues = null;

        public string FilePath { get { return filePath; } set { filePath = value; } }

        public string DataFormat { get { return dataFormat; } set { dataFormat = value; } }
        public string DataTimeFormat { get { return dataTimeFormat; } set { dataTimeFormat = value; } }
        public string TimeFormat { get { return timeFormat; } set { timeFormat = value; } }


        //--------------------------------------------------------------------------------------------------------------------------------
        public HTMLReportsRender(string filePath, string dataFormat, string dataTimeFormat, string timeFormat)
        {
            this.filePath = filePath;

            this.dataFormat = dataFormat;
            this.dataTimeFormat = dataTimeFormat;
            this.timeFormat = timeFormat;
        }

        //---------------------------------------------------------------------
        public void CreateHTMLForMerge(object[,] cellsValues, string[] titles)
        {
           StringWriter outputHtml = new StringWriter();
            HtmlTextWriter writer = new HtmlTextWriter(outputHtml);

            writer.RenderBeginTag(HtmlTextWriterTag.Html);

            writer.RenderBeginTag(HtmlTextWriterTag.Head);
            writer.RenderBeginTag(HtmlTextWriterTag.Title);
            writer.Write("Esportazione Woorm");
            writer.RenderEndTag(); // TITLE

            //// includo un foglio di stile CSS
            //writer.AddAttribute(HtmlTextWriterAttribute.Href, "TreeMenu.css");
            //writer.AddAttribute(HtmlTextWriterAttribute.Rel, "stylesheet");
            //writer.AddAttribute(HtmlTextWriterAttribute.Type, "text/css");
            //writer.RenderBeginTag(HtmlTextWriterTag.Link);
            //writer.RenderEndTag(); // LINK

            writer.RenderEndTag(); // HEAD

            writer.RenderBeginTag(HtmlTextWriterTag.Body);
           
            writer.AddAttribute(HtmlTextWriterAttribute.Style, "font-family:Verdana;");

            writer.AddAttribute(HtmlTextWriterAttribute.Border, "2");
            writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "5");
            writer.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
            writer.RenderBeginTag(HtmlTextWriterTag.Table);
           // align="center" border="2" cellspacing="0" cellpadding="5" width="100%"


            this.titles = titles;
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            writer.AddAttribute(HtmlTextWriterAttribute.Style, "padding:10px;text-align:center;");
           
            for (int i = 0; i < titles.Length; i++)
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.AddAttribute(HtmlTextWriterAttribute.Style, "padding:10px;text-align:center;");
                writer.RenderBeginTag(HtmlTextWriterTag.B);
                writer.Write(titles[i]);
                writer.RenderEndTag(); //</b>
                writer.RenderEndTag(); // </td>
            }

            writer.RenderEndTag(); // TR

            this.cellsValues = cellsValues;

            if (cellsValues == null)
                return;

            int bound0 = cellsValues.GetUpperBound(0);
            int bound1 = cellsValues.GetUpperBound(1);

            for (int i = 0; i <= bound0; i++)
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                writer.AddAttribute(HtmlTextWriterAttribute.Style, "padding-right:20px;");
                for (int x = 0; x <= bound1; x++)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.AddAttribute(HtmlTextWriterAttribute.Style, "padding-right:20px;");
                    writer.Write(cellsValues[i, x].ToString());
                    writer.RenderEndTag(); // </td>
                }

                writer.RenderEndTag(); // TR
            }

            writer.RenderEndTag(); // TABLE
            writer.RenderEndTag(); // BODY
            writer.RenderEndTag(); // HTML

            using (System.IO.FileStream fs = CreateStream(filePath))
            {
                if (fs == null) return;
                Byte[] info = new UTF8Encoding(true).GetBytes(outputHtml.GetStringBuilder().ToString());
                fs.Write(info, 0, info.Length);
            }
        }

        private FileStream CreateStream(string filePath)
        {
            System.IO.FileStream fs;

            if (string.IsNullOrEmpty(filePath))
            {
                SaveFileDialog open = new SaveFileDialog();
                open.Filter = "HTML files (*.html)|*.html|All files (*.*)|*.*";
                open.ShowDialog();
                if (String.IsNullOrEmpty(open.FileName))
                    return null;
                else
                    fs = System.IO.File.Create(open.FileName);
            }
            else
                fs = System.IO.File.Create(filePath);

            return fs;
        }

        //Verifica se fare così o in altro modo LARA
        //---------------------------------------------------------------------
        public void Dispose()
        {

        }
    }

    //=========================================================================
    public class TxtReportsRender : IDisposable
    {

        private string  filePath = string.Empty;
        private string  dataFormat = string.Empty;
        private string  dataTimeFormat = string.Empty;
        private string  timeFormat = string.Empty;
        private int     fileFormat;
        private bool    csvEncoding = false;
        private bool    isClipboard = false;

        private string[] titles = null;
        private object[,] cellsValues = null;

        private string separator = ",";

        public string   FilePath { get { return filePath; } set { filePath = value; } }
        public string   DataFormat { get { return dataFormat; } set { dataFormat = value; } }
        public string   DataTimeFormat { get { return dataTimeFormat; } set { dataTimeFormat = value; } }
        public string   TimeFormat { get { return timeFormat; } set { timeFormat = value; } }
        public int      FileFormat { get { return fileFormat; } set { fileFormat = value; } }
        public string Separator { get { return separator; } set { separator = value; } }

        //--------------------------------------------------------------------------------------------------------------------------------
        public TxtReportsRender(string filePath, string dataFormat, string dataTimeFormat, string timeFormat, int fileFormat, bool csvEncoding, bool isClipboard, string separator)
        {
            this.filePath       = filePath;
            this.dataFormat     = dataFormat;
            this.dataTimeFormat = dataTimeFormat;
            this.timeFormat     = timeFormat;
            this.fileFormat     = fileFormat;
            this.csvEncoding    = csvEncoding;
            this.isClipboard    = isClipboard;
            this.separator      = separator;
        }

        //---------------------------------------------------------------------
        public void CreateTxtForMerge(object[,] cellsValues, string[] titles)
        {

            StringBuilder stringBuilder = new StringBuilder();

            this.titles = titles;

            string stringToAppend = String.Empty;

            for (int i = 0; i < titles.Length; i++)
            {
                if (String.IsNullOrEmpty(titles[i]) || titles[i] == " ")
                    continue;

                if (String.IsNullOrEmpty(stringToAppend))
                    stringToAppend = titles[i] + separator;
                else
                stringToAppend = stringToAppend + " " + titles[i] + separator;
            }

            if (!String.IsNullOrEmpty(stringToAppend))
                stringBuilder.AppendLine(stringToAppend.Substring(0, stringToAppend.Length - 1));

            this.cellsValues = cellsValues;

            if (cellsValues == null)
                return;

            int bound0 = cellsValues.GetUpperBound(0);
            int bound1 = cellsValues.GetUpperBound(1);

            for (int i = 0; i <= bound0; i++)
            {
                stringToAppend = string.Empty;

                for (int x = 0; x <= bound1; x++)
                {
                    if (csvEncoding)
                    {
                        cellsValues[i, x].ToString().Replace("\"", "\"\"");
                        cellsValues[i, x] = '"' + cellsValues[i, x].ToString() + '"';
                    }

                    if (String.IsNullOrEmpty(stringToAppend))
                        stringToAppend = cellsValues[i, x].ToString() + separator;
                    else
                        stringToAppend = stringToAppend + " " + cellsValues[i, x].ToString() + separator;
                }

                stringBuilder.AppendLine(stringToAppend.Substring(0, stringToAppend.Length -1 ));
            }

            if (isClipboard)
                Clipboard.SetText(stringBuilder.ToString());
            else
            {
                using (System.IO.StreamWriter fs = CreateStream(filePath))
                {
                    if (fs == null)
                        return;
                    fs.Write(stringBuilder.ToString());
                }
            }
        }

        //---------------------------------------------------------------------
        private System.Text.Encoding GetEncoding(int encodeIndex)
        { 
            switch(encodeIndex)
            {
                case 0:
                    return System.Text.Encoding.ASCII;
                case 1:
                    return System.Text.Encoding.UTF8;
                case 2:
                    return System.Text.Encoding.BigEndianUnicode;
                case 3:
                    return System.Text.Encoding.Unicode;

            }

            return System.Text.Encoding.UTF8;
        }


        //---------------------------------------------------------------------
        private StreamWriter CreateStream(string filePath)
        {
            System.IO.StreamWriter fs;

            if (string.IsNullOrEmpty(filePath))
            {
                SaveFileDialog open = new SaveFileDialog();
                open.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                open.ShowDialog();
                if (String.IsNullOrEmpty(open.FileName))
                    return null;
                else
                    fs = new System.IO.StreamWriter(open.FileName, false, GetEncoding(fileFormat));
            }
            else
                fs = new System.IO.StreamWriter(filePath, false, GetEncoding(fileFormat));

            return fs;
        }

        //Verifica se fare così o in altro modo LARA
        //---------------------------------------------------------------------
        public void Dispose()
        {

        }
    }

    //=========================================================================
	public class SharpPlaceHolderExportInfo
	{
		private string tagID = string.Empty;
		private string columnFilterName = string.Empty;
		private string[] selectedColumnName = null;
		private object filterValue = null;
		private List<int> columnIndex;
		private int columnFilterIndex;
        private bool isLike = false;

		public List<int> ColumnIndex { get { return columnIndex; } set { columnIndex = value; } }
        public bool IsLike { get { return isLike; } set { isLike = value; } }
		public string TagID { get { return tagID; } set { tagID = value; } }
		public string ColumnFilterName { get { return columnFilterName; } set { columnFilterName = value; } }
		public string[] SelectedColumnName { get { return selectedColumnName; } set { selectedColumnName = value; } }
		public object FilterValue { get { return filterValue; } set { filterValue = value; } }
		public int ColumnFilterIndex { get { return columnFilterIndex; } set { columnFilterIndex = value; } }

		//-------------------------------------------------------------------------------
		public SharpPlaceHolderExportInfo(string tagID, string columnFilterName, string[] selectedColumnName, object filterValue)
		{
			this.tagID = tagID;

			this.columnFilterName = columnFilterName;
			this.selectedColumnName = selectedColumnName;
			this.filterValue = filterValue;
		}

		//-------------------------------------------------------------------------------
		public SharpPlaceHolderExportInfo()
		{
			
		}
	}

	//=========================================================================
	public class DocxReportsRender 
	{
		private string filePath = string.Empty;
		private string dataFormat = string.Empty;
		private string dataTimeFormat = string.Empty;
		private string timeFormat = string.Empty;
		private List<SharpPlaceHolderExportInfo> placeHolders;
        private bool showTotals = false;
        private WordprocessingDocument myDocument = null;
		private string[] titles = null;
		private List<int> columnIndex = null;
		private object[,] cellsValues = null;
        private bool    repeatTitles = false;
        private Microsoft.Office.Interop.Word.Application app;

		public string FilePath { get { return filePath; } set { filePath = value; } }
		public List<int> ColumnIndex { get { return columnIndex; } set { columnIndex = value; } }
		public string DataFormat { get { return dataFormat; } set { dataFormat = value; } }
		public string DataTimeFormat { get { return dataTimeFormat; } set { dataTimeFormat = value; } }
		public string TimeFormat { get { return timeFormat; } set { timeFormat = value; } }
        
        public bool ShowTotals { get { return showTotals; } set { showTotals = value; } }
        public bool     RepeatTitles { get { return repeatTitles; } set { repeatTitles = value; } }

        //---------------------------------------------------------------------
        public DocxReportsRender(string filePath, string dataFormat, string dataTimeFormat, string timeFormat, bool repeatTitles,  bool showTotals)
		{
			this.filePath = filePath;
            this.repeatTitles = repeatTitles;
			this.dataFormat = dataFormat;
			this.dataTimeFormat = dataTimeFormat;
			this.timeFormat = timeFormat;
            this.showTotals = showTotals;
		}

		//---------------------------------------------------------------------
        public void CreateDocx(object[,] cellsValues, string[] titles, object[] tot, int[] rowsForPage)
		{
			this.titles = titles;
			this.cellsValues = cellsValues;
			
			WordprocessingDocument myDocument = null;
			MainDocumentPart mainPart = null;
			Body body = null;
            Paragraph para = null;

            try
            { 
                app = new Microsoft.Office.Interop.Word.Application();
            }
            catch (Exception)
            {
                MessageBox.Show(Strings.NoWord);
                return;
            }

            app.Visible = true;
            
			if (string.IsNullOrEmpty(filePath))
			{

                filePath = System.AppDomain.CurrentDomain.BaseDirectory.Substring(0, 3);
                filePath = Path.Combine(filePath, "Export");
                if (!Directory.Exists(filePath))
                    Directory.CreateDirectory(filePath);
                filePath = Path.Combine(filePath, "Export" + DateTime.Now.ToFileTimeUtc().ToString() + ".docx");
                
                myDocument = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document);
                mainPart = myDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                body = mainPart.Document.AppendChild(new Body());
                para = body.AppendChild(new Paragraph());
            }
			else
			{
                if (!File.Exists(filePath))
                {
                    MessageBox.Show("");//Strings.FileNotExist, Strings.Error);
                    return;
                }
                try
                {
                    myDocument = WordprocessingDocument.Open(filePath, true);
                    BreackPage(myDocument);
                }
                catch(Exception)
                {
                    MessageBox.Show("");//Strings.OpenFIleForAppendError, Strings.Error);
                    return;
                }
                
			}

            Table table = CreateTable();
			TableRow tr = null;
            	
			int bound0 = cellsValues.GetUpperBound(0);
			int bound1 = cellsValues.GetUpperBound(1);

            int maxforpage = 0;
            int pageindex  = 0;

			for (int i = 0; i <= bound0; i++)
			{
                if (repeatTitles && maxforpage == rowsForPage[pageindex])
                {       
                    myDocument.MainDocumentPart.Document.Body.Append(table);
                    BreackPage(myDocument);
                    table = CreateTable();

                    pageindex = pageindex + 1;
                    maxforpage = 0;
                }

				tr = new TableRow();
                string cellvalue;

                for (int x = 0; x <= bound1; x++)
                {
                    cellvalue = cellsValues[i, x].ToString();
                    if (cellsValues[i, x] is DateTime)
                        cellvalue = ((DateTime)cellsValues[i, x]).ToShortDateString();
                    CreateCell(new Text(cellvalue), tr);
                }
				
				table.Append(tr);
                maxforpage = maxforpage + 1;
			}

            if (showTotals)
                CreateTotalsRow(tot, table);
                        
			myDocument.MainDocumentPart.Document.Body.Append(table);
            myDocument.MainDocumentPart.Document.Save();
            
			myDocument.Close();
            app.Documents.Open(filePath);
           

		}

        //---------------------------------------------------------------------
        public void PrintDocument()
        {
            app.ActiveDocument.PrintOutOld();
        }
        //---------------------------------------------------------------------
        private Table CreateTable()
        {
            Table table = new Table();
            table.AppendChild<TableProperties>(CreateTableProperties());
            CreateTitleRow(table);

            return table;
        }

        //---------------------------------------------------------------------
        private void CreateTotalsRow(object[] tot, Table table)
        {
            if (tot == null || tot.Length == 0)
                return;

            TableRow tr = new TableRow();

            for (int i = 0; i < tot.Length; i++)
                CreateCell(new Text(tot[i].ToString()), tr);

            table.Append(tr);
        }

        //---------------------------------------------------------------------
        private void CreateCell(Text text, TableRow tr)
        {
            TableCell  tc1 = new TableCell();
            tc1.Append(new Paragraph(new Run(text)));
            tr.Append(tc1);
        }

        //---------------------------------------------------------------------
        private void BreackPage(WordprocessingDocument myDocument)
        {
            Paragraph PageBreakParagraph = new Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Break() { Type = BreakValues.Page }));
            myDocument.MainDocumentPart.Document.Body.Append(PageBreakParagraph);
        }

        //---------------------------------------------------------------------
        private void CreateTitleRow(Table table) 
        {
            TableRow tr = new TableRow();
            TableCell tc1 = null;

            foreach (string title in titles)
            {
                tc1 = new TableCell();
                Paragraph p = new Paragraph();
                Run run = p.AppendChild(new Run());
                RunProperties runProperties = run.AppendChild(new RunProperties(new Bold()));
                //RunFonts runFont = new RunFonts();
                //runFont.Ascii = "Buxton Sketch";
                //runProperties.Append(runFont);
                runProperties.Append(new Bold());
                FontSize runFont = new FontSize();
                runFont.Val = "28";
                runProperties.Append(runFont);
        //        runProperties.Append(new FontSize() { Val = "20" });
                run.AppendChild(new Text(title));
                tc1.Append(p);
                tr.Append(tc1);

            }

            table.Append(tr);
        }
        //---------------------------------------------------------------------
        private TableProperties CreateTableProperties()
        {
        
			// Create a TableProperties object and specify its border information.
			TableProperties tblProp = new TableProperties(
				new TableBorders(
					new TopBorder()
					{
						Val =
							new EnumValue<BorderValues>(BorderValues.BasicThinLines),
						Size = 12
					},
					new BottomBorder()
					{
						Val =
							new EnumValue<BorderValues>(BorderValues.BasicThinLines),
						Size = 12
					},
					new LeftBorder()
					{
						Val =
							new EnumValue<BorderValues>(BorderValues.BasicThinLines),
						Size = 12
					},
					new RightBorder()
					{
						Val =
							new EnumValue<BorderValues>(BorderValues.BasicThinLines),
						Size = 12
					},
					new InsideHorizontalBorder()
					{
						Val =
							new EnumValue<BorderValues>(BorderValues.BasicThinLines),
						Size = 12
					},
					new InsideVerticalBorder()
					{
						Val =
							new EnumValue<BorderValues>(BorderValues.BasicThinLines),
						Size = 12
					}
				)
			);

            return tblProp;
        }

		//---------------------------------------------------------------------
		public void CreateDocxForMerge(object[,] cellsValues, string[] titles, List<SharpPlaceHolderExportInfo> placeHolders, object[] tot)
		{
			
			this.titles = titles;
			this.cellsValues = cellsValues;
			this.placeHolders = placeHolders;
			
			MainDocumentPart mainPart = null;
			Body body = null;

			if (string.IsNullOrEmpty(filePath))
			{
				filePath = "c:\\mydoc.docx";	//TODO path temporaneo
				myDocument = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document);
				mainPart = myDocument.AddMainDocumentPart();
				mainPart.Document = new Document();
				body = mainPart.Document.AppendChild(new Body());
				Paragraph para = body.AppendChild(new Paragraph());

			}
			else
				myDocument = WordprocessingDocument.Open(filePath, true);

            Table		table  = null;
            List<int>	indexs = null;
            int bound0 = cellsValues.GetUpperBound(0);
            int bound1 = cellsValues.GetUpperBound(1);
            bool empty = true;


            foreach (SharpPlaceHolderExportInfo placeHolderExportInfo in placeHolders)
            {
                empty = true;
                table = new Table();
                CreateTableProperties(table);
                indexs = CreateTitlesRow(table, placeHolderExportInfo);
                string a = string.Empty;

                if ("C" == placeHolderExportInfo.TagID)
                    a = placeHolderExportInfo.TagID;
                int totCell = 0;
                for (int i = 0; i <= bound0; i++)
                {
                   
                    if (placeHolderExportInfo.IsLike)
                        if (!cellsValues[i, placeHolderExportInfo.ColumnFilterIndex].ToString().Contains(placeHolderExportInfo.FilterValue.ToString()))
                            continue;
                        else
                            a = "";
                    else
                        if (cellsValues[i, placeHolderExportInfo.ColumnFilterIndex].ToString() != placeHolderExportInfo.FilterValue.ToString())
                            continue;
                    string b = a;
                    empty = false;
                    TableRow tr2 = new TableRow();
                    totCell = 0;
                    for (int x = 0; x <= bound1; x++)
                    {
                        if (!placeHolderExportInfo.ColumnIndex.Contains(x))
                            continue;
                        TableCell tc1 = new TableCell();
                        totCell = totCell + 1;
                        Paragraph p = new Paragraph();
                        Run run2 = p.AppendChild(new Run());

                        RunProperties runProperties = run2.AppendChild(new RunProperties());
                        runProperties.AppendChild(new RunFonts() { Ascii = "Verdana" });
                        FontSize fontSize = new FontSize();
                        fontSize.Val = "15";
                        runProperties.AppendChild(fontSize);
                    //    runProperties.Append(new Justification() { Val = JustificationValues.Left });
                        //Justification a  = new Justification();

                        //a.Val = JustificationValues.Left;
                        //runProperties.AppendChild(a);

                        run2.AppendChild(new Text(cellsValues[i, x].ToString()));
                        run2.AppendChild<Justification>(new Justification() { Val = JustificationValues.Left });

                        tc1.Append(p);
                        tr2.Append(tc1);
                    }
                    table.Append(tr2);
                }

                if (!empty)
                {
                    try
                    {
                        //Append TOtali
                        TableRow tr3 = new TableRow();
                        decimal tot2 = 0;
                        TableCell tc2 = null;
                        tc2 = new TableCell();
                        Paragraph p = new Paragraph();
                        Run run2 = p.AppendChild(new Run());

                        RunProperties runProperties = run2.AppendChild(new RunProperties());
                        runProperties.AppendChild(new RunFonts() { Ascii = "Verdana" });
                        FontSize fontSize = new FontSize();
                        fontSize.Val = "15";
                        runProperties.AppendChild(fontSize);
                        run2.AppendChild(new Text(string.Empty));
                        tc2.Append(p);
                        tr3.Append(tc2);
                        for (int ii = 1; ii < totCell; ii++)
                        {
                            tot2 = 0;
                            for (int i = 1; i < table.Elements<TableRow>().Count(); i++)
                            {
                                TableRow row = table.Elements<TableRow>().ElementAt(i);
                                if (row == null)
                                    continue;
                                 
                                decimal integ = Convert.ToDecimal(row.Elements<TableCell>().ElementAt(ii).InnerText.ToString());
                                tot2 = tot2 + integ;
                            }

                            tc2 = new TableCell();
                            p = new Paragraph();
                            run2 = p.AppendChild(new Run());

                            runProperties = run2.AppendChild(new RunProperties());
                            runProperties.AppendChild(new RunFonts() { Ascii = "Verdana" });
                            fontSize = new FontSize();
                            fontSize.Val = "15";
                            runProperties.AppendChild(fontSize);
                            run2.AppendChild(new Text(tot2.ToString()));
                            tc2.Append(p);
                            tr3.Append(tc2);
                        }
                        table.Append(tr3);

                    }
                    catch (Exception exx)
                    {
                        string A = exx.Message;
                    }
                }

                //Prima di scrivere fisicamente la tabella ne modifico le dimensioni
                
                List<OpenXmlElement> sdtList = myDocument.MainDocumentPart.Document.Body.Descendants().ToList();

                foreach (OpenXmlElement sdt in sdtList)
                {
                    if (sdt.InnerXml == placeHolderExportInfo.TagID)
                    {
                        var parent = sdt.Parent;   
                        sdt.Remove();
                        if (!empty)
                        {
                            Run run = new Run(new RunProperties(new Bold()));
                            Paragraph newParagraph = new Paragraph(run);
                            // insert after bookmark parent
                            parent.InsertAfterSelf(newParagraph);
                            newParagraph.InsertAfterSelf(table);
                        }

                        break;
                    }
                }

            }
	
			myDocument.MainDocumentPart.Document.Save();
			myDocument.Close();
		}

		//-------------------------------------------------------------------------
		private void CreateTableProperties(Table table)
		{

			// Create a TableProperties object and specify its border information.
			TableProperties tblProp = new TableProperties(
				new TableBorders(new TopBorder() { Val = new EnumValue<BorderValues>(BorderValues.BasicThinLines), Size = 8 },
									new BottomBorder() { Val = new EnumValue<BorderValues>(BorderValues.BasicThinLines), Size = 8 },
									new LeftBorder() { Val = new EnumValue<BorderValues>(BorderValues.BasicThinLines), Size = 8 },
									new RightBorder() { Val = new EnumValue<BorderValues>(BorderValues.BasicThinLines), Size = 8 },
									new InsideHorizontalBorder() { Val = new EnumValue<BorderValues>(BorderValues.BasicThinLines), Size = 8 },
									new InsideVerticalBorder() { Val = new EnumValue<BorderValues>(BorderValues.BasicThinLines), Size = 8 },
                                   new TableWidth() { Type = TableWidthUnitValues.Pct, Width = "5000" },
                                    new Bold( )));
            //table.AppendChild< TableProperties>(new TableWidth() { Type = TableWidthUnitValues.Pct, Width = "100" });
			// Append the TableProperties object to the emspty table.
			table.AppendChild<TableProperties>(tblProp);
            RunProperties runHeader = new RunProperties();
            runHeader.Append(new Bold());
 		}

		//-------------------------------------------------------------------------
		private List<int> CreateTitlesRow(Table table, SharpPlaceHolderExportInfo placeHolderExportInfo)
		{
			// gia' che filtro le colonne giuste ritorno una lista di interi, in modo tale da avere
			// l'indice delle colonne da visualizzare
			TableRow tr = new TableRow();
			TableCell tc1 = null;
			Paragraph p = null;
			List<int> indexs = new List<int>();

          	for(int i=0; i<titles.Length; i++)
			{
				foreach (int column in placeHolderExportInfo.ColumnIndex)
				{
					if (i ==  column)
					{

                        tc1 = new TableCell();
                        p = new Paragraph();
                        Run run2 = p.AppendChild(new Run());

                        RunProperties runProperties = run2.AppendChild(new RunProperties());
                        runProperties.AppendChild(new RunFonts() { Ascii = "Verdana" });
                        FontSize fontSize = new FontSize();
                        fontSize.Val = "15";
                        runProperties.AppendChild(fontSize);
                        //Bold bold = new Bold();
                        //bold.Val = OnOffValue.FromBoolean(true);
                        //runProperties.AppendChild(bold);
                        runProperties.AppendChild<Justification>(new Justification() { Val = JustificationValues.Left });
						run2.AppendChild(new Text(titles[i] ));
                        tc1.Append(p);
						tr.Append(tc1);
						indexs.Add(i);

						break;
					}
				}
			}
          	table.Append(tr);
			return indexs; 
		}
	}

    //=========================================================================
    public class PDFRender
    {
        private TBPicPDF nativePDF = new TBPicPDF();

        public PDFRender(string filePath, string dataFormat, string dataTimeFormat, string timeFormat, bool repeatTitles, bool showTotals)
        {
            //this.filePath = filePath;
            //this.repeatTitles = repeatTitles;
            //this.dataFormat = dataFormat;
            //this.dataTimeFormat = dataTimeFormat;
            //this.timeFormat = timeFormat;
            //this.showTotals = showTotals;


        }

    }
}
