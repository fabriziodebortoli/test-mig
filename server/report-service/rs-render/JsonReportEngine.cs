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
using DocumentFormat.OpenXml.Spreadsheet;
using Microarea.RSWeb.Objects;

namespace Microarea.RSWeb.Render
{
    public class JsonReportEngine
    {
        public TbReportSession ReportSession;

        public RSEngine StateMachine = null;

        private int pageNum = 1;

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
                        msg.page = msg.page;
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

                /*
                 case MessageBuilder.CommandType.ABORTASK:  //click on CANCEL
                    {
                            Debug.Assert(StateMachine.CurrentState == State.RenderingForm);

                            StateMachine.CurrentState = State.ExecuteUserBreak;
                            StateMachine.Step();
                        }
                 * */

                case MessageBuilder.CommandType.INITTEMPLATE:
                    {
                        msg.message = GetJsonInitTemplate();
                        break;
                    }
                case MessageBuilder.CommandType.TEMPLATE:
                    {
                        if (int.TryParse(msg.page, out pageNum))
                        {
                            msg.message = GetJsonTemplatePage(ref pageNum);
                            msg.page = pageNum.ToString();
                        }
                        break;
                    }

                case MessageBuilder.CommandType.DATA:
                    {
                        msg.page = pageNum.ToString();
                        msg.message = GetJsonDataPage(pageNum);
                        break;
                    }
                case MessageBuilder.CommandType.RERUN:
                    {
                        //reset state machine
                        StateMachine.StopReport();
                        StateMachine.ReportSession.uniqueID = Guid.NewGuid().ToString();
                        StateMachine.CurrentState = State.ExecuteAsk;

                        GetJsonAskDialog(null, "");

                        msg.commandType = MessageBuilder.CommandType.NONE;
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
                        msg.page = pageNum.ToString();
                        msg.message = GetExcelDataPage(pageNum);
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
        public string GetExcelDataPage(int page)
        {
            WoormDocument woorm = StateMachine.Woorm;
            if (StateMachine.Report.EngineType != EngineType.FullExtraction)
                while (!woorm.RdeReader.IsPageReady(page))
                {
                    System.Threading.Tasks.Task.Delay(1000).Wait();
                };
            //TODO RSWEB bloccare prima
            if (page > woorm.RdeReader.TotalPages)
            {
                page = woorm.RdeReader.TotalPages;
            }

            string result = Path.GetTempPath();
            string fileName = result + woorm.Properties.Title.Remove(' ', 0, 0) + ".xlsx";
            
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(fileName, SpreadsheetDocumentType.Workbook))
            {
                //crea il documento Excel
                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                //crea il foglio 
                WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet();

                // Adding style
                WorkbookStylesPart stylePart = workbookPart.AddNewPart<WorkbookStylesPart>();
                stylePart.Stylesheet = GenerateStylesheet();
                stylePart.Stylesheet.Save();

                SheetData sheetData = new SheetData();

                for (int i = 1; i <= woorm.RdeReader.TotalPages; i++)
                {
                    woorm.LoadPage(page);

                    foreach (BaseObj o in woorm.Objects)
                    {
                        if (o is Objects.Table)
                        {
                            Objects.Table t = o as Objects.Table;

                            if (page == 1)
                            {
                                List<DocumentFormat.OpenXml.Spreadsheet.Column> columList = new List<DocumentFormat.OpenXml.Spreadsheet.Column>();
                                List<DocumentFormat.OpenXml.Spreadsheet.Cell> cells = new List<DocumentFormat.OpenXml.Spreadsheet.Cell>();

                                // Setting up columns
                                Columns columns = new Columns();
                                Row row = new Row();
                                uint minCol = 0;
                                uint maxCol = 0;

                                foreach (Objects.Column col in t.Columns)
                                {
                                    minCol++;
                                    maxCol++;
                                    columList.Add(new DocumentFormat.OpenXml.Spreadsheet.Column
                                    {
                                        Min = minCol,
                                        Max = maxCol,
                                        Width = ConvertPixelToMm(col.ColumnCellsRect.Width),
                                        CustomWidth = true
                                    });

                                    // Constructing header
                                    ushort id = col.InternalID;
                                    string title = col.Title.Text;
                                    cells.Add(ConstructCell(title, CellValues.String, 2));
                                }
                                columns.Append(columList);
                                worksheetPart.Worksheet.AppendChild(columns);

                                Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());
                                Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = woorm.Properties.Title };
                                sheets.Append(sheet);

                                workbookPart.Workbook.Save();

                                sheetData = worksheetPart.Worksheet.AppendChild(new SheetData());

                                // Insert the header row to the Sheet Data 
                                row.Append(cells);
                                sheetData.AppendChild(row);
                            }

                            // Inserting each row
                            for (int r = 0; r < t.RowNumber; r++)
                            {
                                Row row = new Row();

                                List<DocumentFormat.OpenXml.Spreadsheet.Cell> dataCells = new List<DocumentFormat.OpenXml.Spreadsheet.Cell>();
                                for (int c = 0; c < t.ColumnNumber; c++)
                                {
                                    Objects.Column colData = t.Columns[c];
                                    string v = colData.Cells[r].Value.FormattedData;
                                    dataCells.Add(ConstructCell(v, CellValues.String, 1));
                                }
                                row.Append(dataCells);
                                sheetData.AppendChild(row);
                            }
                            worksheetPart.Worksheet.Save();
                            page++;
                        }

                    }
                }
            }
            return woorm.Properties.Title.Remove(' ', 0, 0).ToJson();
        }

        private DocumentFormat.OpenXml.Spreadsheet.Cell ConstructCell(string value, CellValues dataType, uint styleIndex = 0)
        {
            return new DocumentFormat.OpenXml.Spreadsheet.Cell()
            {
                CellValue = new CellValue(value),
                DataType = new EnumValue<CellValues>(dataType),
                StyleIndex = styleIndex
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

            DocumentFormat.OpenXml.Spreadsheet.Borders borders = new DocumentFormat.OpenXml.Spreadsheet.Borders(
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


    }
}

