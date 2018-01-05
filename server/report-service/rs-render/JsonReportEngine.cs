using System;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;

using Microarea.Common.Applications;
using Microarea.Common.Generic;

using Microarea.RSWeb.WoormViewer;
using Microarea.RSWeb.Models;
using Microarea.RSWeb.WoormEngine;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using spreadsheet = DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microarea.RSWeb.Objects;
using Microarea.Common.CoreTypes;
using TaskBuilderNetCore.Interfaces;
using System.Diagnostics;

namespace Microarea.RSWeb.Render
{
    public class JsonReportEngine
    {
        public TbReportSession ReportSession;

        public RSEngine StateMachine = null;
        Snapshot pagesSnapshot = null;
        //int numPagSnapshot = 0;

        private int pageNum = 1;

        public const string ReportFolderNameFormatter = @"yyyyMMddTHHmmss";

        //--------------------------------------------------------------------------
        public JsonReportEngine(TbReportSession session)
        {
            ReportSession = session;

            session.EngineType = EngineType.FullExtraction;//TODO RSWEB .Paginated_Standard;
        }

        public void Execute()
        {
            StateMachine = new RSEngine(ReportSession);

            StateMachine.Step();

            // se ci sono stati errori li trasmetto nel file XML stesso
            if (StateMachine.HtmlPage == HtmlPageType.Error)
                StateMachine.XmlGetErrors();
        }

        //---------------------------------------------------------------------
        public string GetJsonInitTemplate()
        {
            WoormDocument woorm = StateMachine.Woorm;
            return woorm.ToJson(true, "page", true, StateMachine.ReportTitle);
        }

        public string GetJsonTemplatePage(ref int page)
        {
            WoormDocument woorm = StateMachine.Woorm;

            //TODO RSWEB OTTIMIZZAZIONE sostituire con file system watcher
            if (StateMachine.Report.EngineType != EngineType.FullExtraction)
                while (!woorm.RdeReader.IsPageReady(page))
                {
                    System.Threading.Tasks.Task.Delay(1000).Wait();

                    //if (woorm.RdeReader.LoadTotPage())
                    //    break;
                };  //wait 

            //TODO RSWEB bloccare prima
            if (page > woorm.RdeReader.TotalPages)
            {
                page = woorm.RdeReader.TotalPages;
            }

            woorm.LoadPage(page);

            return woorm.ToJson(true);
        }

        public string GetJsonDataPage(int page = 1)
        {
            WoormDocument woorm = StateMachine.Woorm;

            //TODO RSWEB OTTIMIZZAZIONE sostituire con file system watcher
            if (StateMachine.Report.EngineType != EngineType.FullExtraction)
                while (!woorm.RdeReader.IsPageReady(page))
                {
                    System.Threading.Tasks.Task.Delay(1000).Wait();

                    //if (woorm.RdeReader.LoadTotPage())
                    //    break;
                };  //wait 

            //TODO RSWEB bloccare prima
            if (page > woorm.RdeReader.TotalPages)
            {
                page = woorm.RdeReader.TotalPages;
            }

            woorm.LoadPage(page);

            return woorm.ToJson(false);
        }

        //---------------------------------------------------------------------
        //chiamata sul change delle askentry di tipo "runatserver"
        public string UpdateJsonAskDialog(List<AskDialogElement> values, string currentDialogName)
        {
            AskDialog askDialog = StateMachine.Report.Engine.FindAskDialog(currentDialogName);
            if (askDialog == null)
                return string.Empty;
            if (StateMachine.Report.CurrentAskDialog != askDialog)
                return string.Empty;

            askDialog.AssignAllAskData(values);

            return askDialog.ToJson();
        }

        //---------------------------------------------------------------------
        //chiamata sul NEXT delle askdialog
        public string GetJsonAskDialog(List<AskDialogElement> values, string currentClientDialogName)
        {
            if (currentClientDialogName.IsNullOrEmpty() || values == null || values.Count == 0)
            {
                //TODO RSWEB in caso di RERUN occorre cambiare la cartella dei file temporanei
                //if (StateMachine.CurrentState == State.End)
                //StateMachine.CurrentState = State.ExecuteAsk;

                //viene cercata la prima, se esiste
                StateMachine.Step();
                return string.Empty;
            }

            if (StateMachine.Report.CurrentAskDialog == null || !currentClientDialogName.CompareNoCase(StateMachine.Report.CurrentAskDialog.FormName))
            {
                //il client ha fatto prev
                AskDialog dlg = StateMachine.Report.Engine.GetAskDialog(currentClientDialogName);
                if (dlg != null)
                    StateMachine.Report.CurrentAskDialog = dlg;
            }

            if (StateMachine.Report.CurrentAskDialog == null)
                return string.Empty;

            StateMachine.Report.CurrentAskDialog.AssignAllAskData(values);

            //passa alla prossima dialog se esiste oppure inizia estrazione dati
            StateMachine.Step();

            return string.Empty;
        }

        //---------------------------------------------------------------------
        private string PreviousAskDialog(string currentClientDialogName)
        {
            AskDialog askDialog = StateMachine.Report.Engine.GetAskDialog(currentClientDialogName);
            if (askDialog != null)
                StateMachine.Report.CurrentAskDialog = askDialog;

            return askDialog.ToJson();
        }

        //---------------------------------------------------------------------
        public Message GetResponseFor(Message msg)
        {
            switch (msg.commandType)
            {
                case MessageBuilder.CommandType.ASK:
                    {
                        //contiene il nome della dialog, se è vuota/=="0" viene richiesta la prima per la prima volta
                        /* if (!int.TryParse(msg.page, out pageNum))
                             break;  */
                        List<AskDialogElement> values = msg.page.IsNullOrEmpty() ? null
                                                        : JsonConvert.DeserializeObject<List<AskDialogElement>>(msg.message);

                        GetJsonAskDialog(values, msg.page);

                        msg.commandType = MessageBuilder.CommandType.NONE;
                        break;
                    }

                case MessageBuilder.CommandType.PREVASK:
                    {
                        msg.message = PreviousAskDialog(msg.page);
                        break;
                    }

                case MessageBuilder.CommandType.UPDATEASK:
                    {

                        List<AskDialogElement> values = msg.page.IsNullOrEmpty() ? null
                                                           : JsonConvert.DeserializeObject<List<AskDialogElement>>(msg.message);

                        msg.message = UpdateJsonAskDialog(values, msg.page);

                        break;
                    }

                
                 case MessageBuilder.CommandType.ABORTASK:  //click on CANCEL
                   {
                       Debug.Assert(StateMachine.CurrentState == State.ExecuteAsk);

                       StateMachine.CurrentState = State.ExecuteUserBreak;
                       StateMachine.Step();
                       break;
                   }
                 

                case MessageBuilder.CommandType.INITTEMPLATE:
                    {
                        msg.message = GetJsonInitTemplate();
                        break;
                    }
                case MessageBuilder.CommandType.TEMPLATE:
                    {
                        int pos = msg.page.IndexOf(',');
                        if (pos > -1)
                        {
                            string[] split = msg.page.Split(',');
                            string pagNumber = split[0];
                            string copyNum = split[1];
                            int copies = 1;
                            if (!int.TryParse(pagNumber, out pageNum))
                                break;
                            if (!int.TryParse(copyNum, out copies))
                                break;
                            //TODO RSWEB set copies number into symbol table
                        }
                        else if (!int.TryParse(msg.page, out pageNum))
                            break;

                        if (pagesSnapshot != null)
                        {
                            int idx = (pageNum - 1) * 2;
                            msg.message = pagesSnapshot.pages[idx].ToString();
                        }
                        else
                        {
                            msg.message = GetJsonTemplatePage(ref pageNum);
                        }
                        msg.page = pageNum.ToString();
                        break;
                    }

                case MessageBuilder.CommandType.DATA:
                    {
                        if (pagesSnapshot != null)
                        {
                            int idx = (pageNum - 1) * 2 + 1;
                            msg.message = pagesSnapshot.pages[idx].ToString();
                        }
                        else
                        {
                            msg.message = GetJsonDataPage(pageNum);
                        }
                        msg.page = pageNum.ToString();
                        break;
                    }
                case MessageBuilder.CommandType.RERUN:
                    {
                        StateMachine.ReRun();
                        pageNum = 1;
                        msg.message = GetJsonInitTemplate();
                        msg.commandType = MessageBuilder.CommandType.INITTEMPLATE;
                        break;
                    }

                //----------------------------------------------
                //TODO
                case MessageBuilder.CommandType.STOP:
                    {
                        // this.stateMachine.Do()
                        msg.message = "Executed STOP()";

                        StateMachine.StopReport();
                        break;
                    }

                case MessageBuilder.CommandType.EXPORTEXCEL:
                    {
                        string[] split = msg.page.Split(',');
                        string firstPage = split[0];
                        string lastPage = split[1];
                        int first, last = 0;
                        Int32.TryParse(firstPage, out first);
                        Int32.TryParse(lastPage, out last);
                        msg.message = GetExcelDataPage(first, last);
                        break;
                    }
                /*case MessageBuilder.CommandType.EXPORTDOCX:
                    {
                        msg.page = pageNum.ToString();
                        msg.message = GetDocxDataPage(pageNum);
                        break;
                    }*/
                case MessageBuilder.CommandType.SNAPSHOT:
                    {
                        //il flag user-allUser è passato insieme al numeroPagina
                        bool forAllUsers = false;
                        string[] split = msg.page.Split(',');
                        string name = split[1];
                        string user = split[2];
                        if (user.Equals("true"))
                            forAllUsers = true;
                        SaveSnapshot(name, forAllUsers);
                        msg.commandType = MessageBuilder.CommandType.NONE;
                        break;
                    }
                case MessageBuilder.CommandType.ACTIVESNAPSHOT:
                    {
                        msg.message = ActiveSnapshot();
                        break;
                    }
                case MessageBuilder.CommandType.RUNSNAPSHOT:
                    {
                        //il flag user-allUser è passato insieme al numeroPagina
                        bool forAllUsers = false;
                        string[] split = msg.page.Split(',');
                        string name = split[1];
                        string user = split[2];
                        if (user.Equals("true"))
                            forAllUsers = true;
                        msg.message = RunJsonSnapshot(name, forAllUsers);
                        msg.page = pageNum.ToString();
                        msg.commandType = MessageBuilder.CommandType.SNAPSHOT;
                        break;
                    }
            }
            return msg;
        }

        //---------------------------------------------------------------------
        //per debug
        public string GetJsonAskDialog(int index = 0)
        {
            AskDialog dlg = StateMachine.Report.Engine.GetAskDialog(index);
            if (dlg == null)
                return string.Empty;

            return dlg.ToJson();
        }

        //---------------------------------------------------------------------
        public string GetJsonAskDialogs()
        {
            return StateMachine.Report.Engine.ToJsonDialogs();
        }

        //---------------------------------------------------------------------
        //chiamata per esportare excel
        public string GetExcelDataPage(int firstPage, int lastPage)
        {
            int currentPage = firstPage;
            WoormDocument woorm = StateMachine.Woorm;
            if (StateMachine.Report.EngineType != EngineType.FullExtraction)
                while (!woorm.RdeReader.IsPageReady(firstPage))
                {
                    System.Threading.Tasks.Task.Delay(1000).Wait();
                };
            string result = Path.GetTempPath();
            string fileName = result + woorm.Properties.Title.Remove(' ', 0, 0) + ".xlsx";

            using (SpreadsheetDocument document = SpreadsheetDocument.Create(fileName, SpreadsheetDocumentType.Workbook))
            {
                //crea il documento Excel
                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new spreadsheet.Workbook();

                //crea il foglio 
                WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new spreadsheet.Worksheet();

                // Adding style
                WorkbookStylesPart stylePart = workbookPart.AddNewPart<WorkbookStylesPart>();
                stylePart.Stylesheet = GenerateStylesheet();
                stylePart.Stylesheet.Save();

                spreadsheet.SheetData sheetData = new spreadsheet.SheetData();

                for (int i = firstPage; i <= lastPage; i++)
                {
                    woorm.LoadPage(currentPage);

                    foreach (BaseObj o in woorm.Objects)
                    {
                        if (o is Objects.Table)
                        {
                            Objects.Table t = o as Objects.Table;

                            if (currentPage == firstPage)
                            {
                                List<spreadsheet.Column> columList = new List<spreadsheet.Column>();
                                List<spreadsheet.Cell> cells = new List<spreadsheet.Cell>();

                                // Setting up columns
                                spreadsheet.Columns columns = new spreadsheet.Columns();
                                spreadsheet.Row row = new spreadsheet.Row();
                                uint minCol = 0;
                                uint maxCol = 0;

                                foreach (Objects.Column col in t.Columns)
                                {
                                    minCol++;
                                    maxCol++;
                                    columList.Add(new spreadsheet.Column
                                    {
                                        Min = minCol,
                                        Max = maxCol,
                                        Width = ConvertPixelToMm(col.ColumnCellsRect.Width),
                                        CustomWidth = true
                                    });

                                    // Constructing header
                                    ushort id = col.InternalID;
                                    string title = col.Title.Text;
                                    cells.Add(ConstructCell(title, spreadsheet.CellValues.String, 2));
                                }
                                columns.Append(columList);
                                worksheetPart.Worksheet.AppendChild(columns);

                                spreadsheet.Sheets sheets = workbookPart.Workbook.AppendChild(new spreadsheet.Sheets());
                                spreadsheet.Sheet sheet = new spreadsheet.Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = woorm.Properties.Title };
                                sheets.Append(sheet);

                                workbookPart.Workbook.Save();

                                sheetData = worksheetPart.Worksheet.AppendChild(new spreadsheet.SheetData());

                                // Insert the header row to the Sheet Data 
                                row.Append(cells);
                                sheetData.AppendChild(row);
                            }

                            // Inserting each row
                            for (int r = 0; r < t.RowNumber; r++)
                            {
                                spreadsheet.Row row = new spreadsheet.Row();

                                List<spreadsheet.Cell> dataCells = new List<spreadsheet.Cell>();
                                for (int c = 0; c < t.ColumnNumber; c++)
                                {
                                    Objects.Column colData = t.Columns[c];
                                    string v = colData.Cells[r].Value.FormattedData;

                                    /*if (colData.Cells[r].Value.DataType.CompareNoCase("Number"))
                                        dataCells.Add(ConstructCell(v, CellValues.Number, 3));
                                    else if (colData.Cells[r].Value.DataType.CompareNoCase("Double"))
                                        dataCells.Add(ConstructCell(v, CellValues.Number, 4));
                                    else if (colData.Cells[r].Value.DataType.CompareNoCase("DateTime"))
                                        dataCells.Add(ConstructCell(v, CellValues.Date, 5));
                                    else*/
                                        dataCells.Add(ConstructCell(v, spreadsheet.CellValues.String, 1));
                                }
                                row.Append(dataCells);
                                sheetData.AppendChild(row);
                            }
                            worksheetPart.Worksheet.Save();
                            //currentPage++;

                            if (currentPage == lastPage)
                                return woorm.Properties.Title.Remove(' ', 0, 0).ToJson();
                        }
                        else continue;
                    }
                    currentPage++;
                }
            }
            return "Errore".ToJson();
        }

        private spreadsheet.Cell ConstructCell(string value, spreadsheet.CellValues dataType, uint styleIndex = 0)
        {
            return new spreadsheet.Cell()
            {
                CellValue = new spreadsheet.CellValue(value),
                DataType = new EnumValue<spreadsheet.CellValues>(dataType),
                StyleIndex = styleIndex
            };
        }

        private spreadsheet.Stylesheet GenerateStylesheet()
        {
            spreadsheet.Stylesheet styleSheet = null;

            spreadsheet.Fonts fonts = new spreadsheet.Fonts(
                new spreadsheet.Font( // Index 0 - default
                    new spreadsheet.FontSize() { Val = 10 }

                ),
                new spreadsheet.Font( // Index 1 - header
                    new spreadsheet.FontSize() { Val = 10 },
                    new spreadsheet.Bold(),
                    new spreadsheet.Color() { Rgb = "FFFFFF" }

                ));

            spreadsheet.Fills fills = new spreadsheet.Fills(
                    new spreadsheet.Fill(new spreadsheet.PatternFill() { PatternType = spreadsheet.PatternValues.None }), // Index 0 - default
                    new spreadsheet.Fill(new spreadsheet.PatternFill() { PatternType = spreadsheet.PatternValues.Gray125 }), // Index 1 - default
                    new spreadsheet.Fill(new spreadsheet.PatternFill(new spreadsheet.ForegroundColor { Rgb = new HexBinaryValue() { Value = "66666666" } })
                    { PatternType = spreadsheet.PatternValues.Solid }) // Index 2 - header
                );

            spreadsheet.Borders borders = new spreadsheet.Borders(
                    new spreadsheet.Border(), // index 0 default
                    new spreadsheet.Border( // index 1 black border
                        new spreadsheet.LeftBorder(new spreadsheet.Color() { Auto = true }) { Style = spreadsheet.BorderStyleValues.Thin },
                        new spreadsheet.RightBorder(new spreadsheet.Color() { Auto = true }) { Style = spreadsheet.BorderStyleValues.Thin },
                        new spreadsheet.TopBorder(new spreadsheet.Color() { Auto = true }) { Style = spreadsheet.BorderStyleValues.Thin },
                        new spreadsheet.BottomBorder(new spreadsheet.Color() { Auto = true }) { Style = spreadsheet.BorderStyleValues.Thin },
                        new spreadsheet.DiagonalBorder())
                );

            spreadsheet.NumberingFormats numberingFormats = new spreadsheet.NumberingFormats(
                new spreadsheet.NumberingFormat(), //index 0 default
                new spreadsheet.NumberingFormat { NumberFormatId = 1, FormatCode = StringValue.FromString("#") }, //index 1
                new spreadsheet.NumberingFormat { NumberFormatId = 2, FormatCode = StringValue.FromString("?,???") }, //index 2
                new spreadsheet.NumberingFormat { NumberFormatId = 3, FormatCode = StringValue.FromString("dd/mm/yyyy") } //index 3
            );
            
            spreadsheet.CellFormats cellFormats = new spreadsheet.CellFormats(
                    new spreadsheet.CellFormat(), // default
                    new spreadsheet.CellFormat { FontId = 0, FillId = 0, BorderId = 1, ApplyBorder = true}, // body
                    new spreadsheet.CellFormat { FontId = 1, FillId = 2, BorderId = 1, ApplyFill = true }, // header
                    new spreadsheet.CellFormat { FontId = 0, FillId = 0, BorderId = 1, ApplyBorder = true, NumberFormatId = 1, ApplyNumberFormat = BooleanValue.FromBoolean(true) }, // body numeric
                    new spreadsheet.CellFormat { FontId = 0, FillId = 0, BorderId = 1, ApplyBorder = true, NumberFormatId = 2, ApplyNumberFormat = BooleanValue.FromBoolean(true) }, // body decimal
                    new spreadsheet.CellFormat { FontId = 0, FillId = 0, BorderId = 1, ApplyBorder = true, NumberFormatId = 3, ApplyNumberFormat = BooleanValue.FromBoolean(true) } // body date
                );

            styleSheet = new spreadsheet.Stylesheet(fonts, fills, borders, cellFormats);

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


        /*public string GetDocxDataPage(int pageNum)
        {
            WoormDocument woorm = StateMachine.Woorm;
            if (StateMachine.Report.EngineType != EngineType.FullExtraction)
                while (!woorm.RdeReader.IsPageReady(pageNum))
                {
                    System.Threading.Tasks.Task.Delay(1000).Wait();
                };
            string result = Path.GetTempPath();
            string documentFileName = result + woorm.Properties.Title.Remove(' ', 0, 0) + ".docx";

            // Create a Wordprocessing document
            using (WordprocessingDocument myDoc = WordprocessingDocument.Create(documentFileName, WordprocessingDocumentType.Document))
            {
                woorm.LoadPage(pageNum);

                // Add a new main document part
                MainDocumentPart mainPart = myDoc.AddMainDocumentPart();

                //Create Document tree for simple document
                mainPart.Document = new Document();

                //Create Body (this element contains other elements that we want to include 
                Body body = new Body();

                //Create paragraph 
                Paragraph paragraph = new Paragraph();
                Run run_paragraph = new Run();

                Text text_paragraph = new Text();
                RunProperties rPr = new RunProperties();
         
                

                ParagraphProperties paragraphProperties = new ParagraphProperties();
                ParagraphBorders paragraphBorders = new ParagraphBorders();

               
                List<Paragraph> paragraphList = new List<Paragraph>();
                List<Text> textList = new List<Text>();
            
                foreach (BaseObj o in woorm.Objects)
                {
                  
                    if (o is FieldRect)
                    {
                        paragraph = new Paragraph();
                        run_paragraph = new Run();
                        paragraphBorders = new ParagraphBorders();
                        
                        FieldRect fr = o as FieldRect;
                        if (fr.InternalID > 0x7FF0) continue;
                        
                        string value = fr.Value.FormattedData;
          
                        // we want to put that text into the output document 
                        text_paragraph = new Text(value);
                        //textList.Add(text_paragraph);
                        
                        //Append elements appropriately. 
                        run_paragraph.Append(text_paragraph);
                        
                        if(fr.Borders.Top)
                        {
                            TopBorder topborder = new TopBorder() { Val = BorderValues.Single, Color = fr.DynamicBkgColor.ToString(), Space = (UInt32Value)1U};
                            paragraphBorders.Append(topborder);
                        }
                        if (fr.Borders.Left)
                        {
                            LeftBorder leftborder = new LeftBorder() { Val = BorderValues.Single, Color = fr.DynamicBkgColor.ToString(), Space = (UInt32Value)1U };
                            paragraphBorders.Append(leftborder);
                        }
                        if (fr.Borders.Bottom)
                        {
                            BottomBorder bottomborder = new BottomBorder() { Val = BorderValues.Single, Color = fr.DynamicBkgColor.ToString(), Space = (UInt32Value)1U };
                            paragraphBorders.Append(bottomborder);
                        }
                        if (fr.Borders.Right)
                        {
                            RightBorder rightborder = new RightBorder() { Val = BorderValues.Single, Color = fr.DynamicBkgColor.ToString(), Space = (UInt32Value)1U };
                            paragraphBorders.Append(rightborder);
                        }

                        paragraphProperties.Append(paragraphBorders);

                    }
                    paragraphList.Add(paragraph);
                    paragraph.Append(run_paragraph);

                 

                }
                //paragraphProperties.Append(paragraphBorders);

                paragraph.Append(paragraphProperties);
                
                

                //body.Append(paragraph);
                body.Append(paragraphList);
                mainPart.Document.Append(body);
                
                // Save changes to the main document part. 
                mainPart.Document.Save();
            }
            return woorm.Properties.Title.Remove(' ', 0, 0).ToJson();
        }*/

        //---------------------------------------------------------------------
        //chiamata per snapshot
        public string GetJsonAllPages()
        {
            WoormDocument woorm = StateMachine.Woorm;

            string file = "{ \"pages\":[";

            for (int i = 1; i <= woorm.RdeReader.TotalPages; i++)
            {
                woorm.LoadPage(i);

                if (i > 1) file += ",";

                file += woorm.ToJson(true);
                file += ",";
                file += woorm.ToJson(false);


            }
            file += "]}";
            return file;//ToJson(null, false, false, false);
        }

        public void SaveSnapshot(string name, bool forAllUsers)
        {
            WoormDocument woorm = StateMachine.Woorm;
            string user = "";

            if (!forAllUsers)
                user = ReportSession.UserInfo.User;
            else
                user = NameSolverStrings.AllUsers;

            string customPath = ReportSession.PathFinder.GetCustomReportPathFromWoormFile(woorm.Filename, ReportSession.UserInfo.Company, user);
            string destinationPath = PathFunctions.WoormRunnedReportPath(customPath, Path.GetFileNameWithoutExtension(woorm.Filename), true);
            string pages = GetJsonAllPages();
            string path = destinationPath + DateTime.Now.ToString(ReportFolderNameFormatter) + "_" + name + ".json";

            File.WriteAllText(path, pages);
        }

        public string ActiveSnapshot()
        {
            WoormDocument woorm = StateMachine.Woorm;
            List<string> nameFile = new List<string>();

            string customPath = ReportSession.PathFinder.GetCustomReportPathFromWoormFile(woorm.Filename, ReportSession.UserInfo.Company, ReportSession.UserInfo.User);
            string destinationPath = PathFunctions.WoormRunnedReportPath(customPath, Path.GetFileNameWithoutExtension(woorm.Filename), true);
            DirectoryInfo dUser = new DirectoryInfo(destinationPath);

            string s = "[";
            //bool first = true;

            foreach (FileInfo file in dUser.GetFiles("*.json"))
            {
                string[] split = file.Name.Split('_');
                string date = split[0];
                string nameS = split[1];
                //if (first) first = false;
                //else s += ',';

                DateTime dt;
                bool b = DateTime.TryParse(file.Name, out dt);

                string name = nameS.RemoveExtension(".json");
                s += "{" + false.ToJson("allUsers") + ',' + name.ToJson("name") + ',' + date.ToJson("date") + "},";
            }

            customPath = ReportSession.PathFinder.GetCustomReportPathFromWoormFile(woorm.Filename, ReportSession.UserInfo.Company, NameSolverStrings.AllUsers);
            destinationPath = PathFunctions.WoormRunnedReportPath(customPath, Path.GetFileNameWithoutExtension(woorm.Filename), true);
            DirectoryInfo dAllUser = new DirectoryInfo(destinationPath);

            //first = true;
            foreach (FileInfo file in dAllUser.GetFiles("*.json"))
            {
                string[] split = file.Name.Split('_');
                string date = split[0];
                string nameS = split[1];
                //if (first) first = false;
                //else s += ',';

                DateTime dt;
                bool b = DateTime.TryParse(file.Name, out dt);

                string name = nameS.RemoveExtension(".json");
                s += "{" + true.ToJson("allUsers") + ',' + name.ToJson("name") + ',' + date.ToJson("date") + "},";
            }
            s = s.Remove(s.Length - 1);

            s += "]";
            return s;
        }

        public string RunJsonSnapshot(string name, bool forAllUsers)
        {
            string user = "";
            if (!forAllUsers)
                user = ReportSession.UserInfo.User;
            else
                user = NameSolverStrings.AllUsers;

            string customPath = ReportSession.PathFinder.GetCustomReportPathFromWoormFile(ReportSession.FilePath, ReportSession.UserInfo.Company, user);
            string completePath = PathFunctions.WoormRunnedReportPath(customPath, Path.GetFileNameWithoutExtension(ReportSession.FilePath), true);

            using (StreamReader r = new StreamReader(completePath + name + ".json"))
            {
                string json = r.ReadToEnd();
                pagesSnapshot = JsonConvert.DeserializeObject<Snapshot>(json);
                pageNum = pagesSnapshot.pages.Length / 2;
                return pagesSnapshot.pages[0].ToString();
            }
        }

    }
}
