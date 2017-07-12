using System;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;

using Microarea.Common.Applications;
using Microarea.Common.Generic;

using Microarea.RSWeb.WoormViewer;
using Microarea.RSWeb.Models;
using Microarea.RSWeb.WoormEngine;
using System.Xml;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microarea.RSWeb.Objects;
using System.Linq;

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
        public string GetExcelDataPage(int page = 1)
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

            woorm.LoadPage(page);

            string result = Path.GetTempPath();

            string fileName = result+"Report.xlsx";
            foreach (BaseObj o in woorm.Objects)
            {
                if (o is RSWeb.Objects.Table)
                    using (SpreadsheetDocument document = SpreadsheetDocument.Create(fileName, SpreadsheetDocumentType.Workbook))
                    {
                        WorkbookPart workbookPart = document.AddWorkbookPart();
                        workbookPart.Workbook = new Workbook();

                        WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                        worksheetPart.Worksheet = new Worksheet();

                        //// Adding style
                        //WorkbookStylesPart stylePart = workbookPart.AddNewPart<WorkbookStylesPart>();
                        //stylePart.Stylesheet = GenerateStylesheet();
                        //stylePart.Stylesheet.Save();

                        workbookPart.Workbook.Save();

                        SheetData sheetData = worksheetPart.Worksheet.AppendChild(new SheetData());

                        // Constructing header
                        Row row = new Row();


                        RSWeb.Objects.Table t = o as RSWeb.Objects.Table;

                        foreach (RSWeb.Objects.Column col in t.Columns)
                        {
                            //column header
                            ushort id = col.InternalID;
                            string strType = col.GetDataType();
                            string title = col.Title.Text;

                            row.Append(ConstructCell(title, CellValues.String, 2));
                        }
                        // Insert the header row to the Sheet Data
                        sheetData.AppendChild(row);


                        // Inserting each row
                        for (int r = 0; r < t.RowNumber; r++)
                        {
                            for (int c = 0; c < t.ColumnNumber; c++)
                            {
                                row = new Row();
                                RSWeb.Objects.Column col2 = t.Columns[c];

                                string v = col2.Cells[r].Value.Text;
                                // ??? manca il cellValues

                                row.Append(ConstructCell(v, CellValues.String, 1));

                                sheetData.AppendChild(row);

                            }
                        }
                        worksheetPart.Worksheet.Save();
                    }
            }
            return woorm.Filename.ToString();
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

    }
}

