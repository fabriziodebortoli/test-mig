using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.SessionState;

using Microarea.TaskBuilderNet.Core.OpenXML;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Woorm.WoormController;

using RSjson;

namespace Microarea.TaskBuilderNet.Woorm.WoormWebControl
{
    class XlsHandler : IHttpHandler, IReadOnlySessionState
    {
        /// <summary>
        /// Handler for Xls exporting
        /// </summary>
        /// <param name="context">The current HTTP context</param>
        //--------------------------------------------------------------------------------
        void IHttpHandler.ProcessRequest(System.Web.HttpContext context)
        {
			ReportController controller = (ReportController)context.Session[context.Request.QueryString["SessionTag"]];
			if (controller == null || controller.StateMachine == null)
			{
				context.Response.ContentType = "text/html";
				context.Response.Write(WoormWebControlStrings.SessionExpired);
				return;
			}

            controller.StateMachine.ReportSession.UserInfo.SetCulture();

            WoormViewer.WoormDocument woorm = controller.StateMachine.Woorm;

            XlsRender render = new XlsRender(woorm);
            string st = string.Empty;
            int current = woorm.RdeReader.CurrentPage;

            bool ok = render.LoadWoormTable();
            if (!ok)
            {
                context.Response.ContentType = "text/html";
                context.Response.Write(WoormWebControlStrings.EngineError);
                return;
            }

            if (current != woorm.RdeReader.CurrentPage)
            {
                woorm.LoadPage(current);
                woorm.RdeReader.CurrentPage = current;
            }

            //st = render.ToCSV();    // ToHtml(); ExportToExcel();
            //if (st == string.Empty)
            //    return;

            string filePath = System.IO.Path.GetTempFileName();
            FileInfo f = new FileInfo(filePath);
            render.SaveToFileAndClose(filePath);

            //se ho il titolo del report uso quello come nome del file (rimuovendo gli spazi che danno fastidio ad alcuni browser (mozilla 3.6.10))
            string sTitle = woorm.Properties != null && !string.IsNullOrEmpty(woorm.Properties.Title) ? woorm.Properties.Title.Replace(" ", "") : string.Empty;
            if (string.IsNullOrWhiteSpace(sTitle))
            {
                sTitle = woorm.Namespace.Report.Substring(0, woorm.Namespace.Report.Length - 4);
            }
            string fileName = !string.IsNullOrEmpty(sTitle) ? sTitle : filePath;

            context.Response.AddHeader("Content-Disposition", string.Format("attachment; filename={0}.xlsx", fileName));

            //context.Response.Charset = "UTF-8";
            //context.Response.ContentEncoding = System.Text.Encoding.UTF8;

            //context.Response.AddHeader("Content-Length", f.Length.ToString());
            //context.Response.AddHeader("Content-Length", st.Length.ToString());
           
            //context.Response.ContentType = "application/vnd.xls";
            //context.Response.ContentType = "application/vnd.ms-excel";
            context.Response.ContentType = "application/vnd.openxmlformats";
            //context.Response.ContentType = "text/csv";
             
            context.Response.WriteFile(filePath, true);
            //context.Response.Write(st);
            
            File.Delete(filePath);
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }

    /// <summary>
    /// Xls Renderer
    /// </summary>
    /// ================================================================================
    public class XlsRender
    {
        private WoormViewer.WoormDocument woorm;

		RSjson.Table woormTable = null;
        DataTable dataTable = new DataTable();

        public XlsRender(WoormViewer.WoormDocument woorm)
        {
            this.woorm = woorm;
        }

        //------------------------------------------------------------------------------
        internal void SaveToFileAndClose(string exportFile)
        {
            ExcelExport export = new ExcelExport();
            export.ExportDataTable(dataTable, exportFile);
        }


        //------------------------------------------------------------------------------
        /*
         * * Response.ContentType = "text/csv"
         *  si ricorre alla codifica che segue: 
         *  se il valore contiene caratteri di fine linea,
         *  il carattere separatore o i doppi apici ("), 
         *  esso viene racchiuso tra doppi apici e quelli eventualmente presenti
         *  nel valore sono raddoppiati
           */
        const char sepCSV = ';';

        internal string ToCSV()
        {
           System.Text.StringBuilder sb = new System.Text.StringBuilder();

           foreach (DataColumn col in dataTable.Columns)
           {
               sb.Append(col.Caption.ToCSV(sepCSV) + sepCSV);
           }
           sb.Append("\r\n");

           foreach (DataRow row in dataTable.Rows)
            {
                bool notFirst = false;
                foreach (DataColumn col in dataTable.Columns)
                {
                    if (notFirst)
                        sb.Append(sepCSV);
                    else
                        notFirst = true;

                    string s = row[col].ToString();
                    sb.Append(s.ToCSV(sepCSV));
                }
                sb.Append("\r\n");
            }
            return sb.ToString();
        }

        internal string ToHtml()
        {
            System.IO.StringWriter stringWrite = new System.IO.StringWriter();
            System.Web.UI.Html32TextWriter htmlWrite = new System.Web.UI.Html32TextWriter(stringWrite);

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("<table border=\"1\">");
            foreach (DataRow row in dataTable.Rows)
            {
                sb.Append("<tr>");
                foreach (DataColumn col in dataTable.Columns) 
                {        
                    sb.Append("<td>" + row[col].ToString() + "</td>");      
                }      
                sb.Append("</tr>");    
            }    
            sb.Append("</table");    
            
            htmlWrite.Write(sb);    

            return stringWrite.ToString();
        }

        public string ExportToExcel()
        {
            using (StringWriter sw = new StringWriter())
            {
                using (System.Web.UI.HtmlTextWriter htw = new System.Web.UI.HtmlTextWriter(sw))
                {
                    System.Web.UI.WebControls.GridView dg = new System.Web.UI.WebControls.GridView();
                    dg.DataSource = dataTable;
                    dg.DataBind();
                    dg.RenderControl(htw);
                    return sw.ToString();
                }
            }
         }
       

        ///////////////////////////////////////////////////////////////////////
		// Scorro gli oggetti del Documento di Woorm finchè non trovo la tabella selezionata
        bool FindTable(ushort idT = 0)
        {
            for (int i = 0; i < woorm.Objects.Count; i++)
            {
				if (woorm.Objects[i] is RSjson.Table)
                {
					RSjson.Table wt = woorm.Objects[i] as RSjson.Table;
                    if (idT == 0 || wt.InternalID == idT)
                    {
                        woormTable = wt;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool LoadWoormTable()
        {	
 	        try 
	        {	
                if (!FindTable())
                    return false;
                ushort idTable = woormTable.InternalID;

                bool outputTitle = true;

                for (int nPageIdx = 1; nPageIdx <= woorm.RdeReader.TotalPages; nPageIdx++)
                {
                    woorm.LoadPage(nPageIdx);
                    if (!FindTable(idTable))
                        continue;
			        woorm.SynchronizeSymbolTable();
                    
                    if (outputTitle)
                    {
                        ColumnsType = new System.String[woormTable.ColumnNumber];
                        ColumnsID = new ushort[woormTable.ColumnNumber]; ;
                        DataColumnsIdx = new short[woormTable.ColumnNumber]; ;

			            //esportazione titoli
                    	for (int nColIdx = 0; nColIdx < woormTable.ColumnNumber; nColIdx++)
				        {
                            Column tCol = woormTable.Columns[nColIdx];

                            if (tCol.HideExpr == null && tCol.IsHidden)
                                continue;
                            if (tCol.HideExpr != null)
                            {
                               Value bHidden = tCol.HideExpr.Eval();
                                if (bHidden.Valid && ObjectHelper.CastBool(bHidden.Data))
                                    continue;
                            }

                            AddColum(nColIdx);
				        } 
				        outputTitle = false;
			        }

			        for (int nRowIdx = 0; nRowIdx < woormTable.RowNumber; nRowIdx++)
			        {
				        woorm.SynchronizeSymbolTable(nRowIdx);

				        bool bValidRow = false;

				        for (int nColIdx = 0; nColIdx < woormTable.ColumnNumber; nColIdx++)
				        {
                            Column col = woormTable.Columns[nColIdx];

                            if (nColIdx >= ColumnsID.Count())
                            {
                                continue;
                            }

                            if (col.InternalID != ColumnsID[nColIdx])
                            {
                                continue;
                            }

                             if (ColumnsType[nColIdx] == null)   //AddColum set null on column has unrecognized data type
                                continue;

                           if (col.HideExpr == null && col.IsHidden)
                                continue;

					        object value;
					        bool isTailMultiLine;

					        bool bOk = GetCellData 
							                ( 
 								                nRowIdx, nColIdx,
								                out value,
                                                out isTailMultiLine
							                ) ;
                            
                             
                            if (bOk && value != null)
					        {
						        if (!isTailMultiLine)
						        {
							        if (!bValidRow)
							        {
								        NewRow ();
 
								        bValidRow = true;
							        }
						
							        AddCellValue(nColIdx, value);
						        }
						        else if (isTailMultiLine)
						        {
							        AppendCellValue (nColIdx, value);
						        }
					        }					
				        } //end for colonne
			        } //end for righe di una pagina
		        }//end for pagine	

		        return true;
	        } 
	        catch (System.Exception ex) 
	        {
                string s = ex.Message;
	        }
	        return false;
        }

       //------------------------------------------------------------------------------
        string [] ColumnsType = null;
        ushort[] ColumnsID = null;
        short[] DataColumnsIdx = null;

        bool AddColum(int nColIdx)
        {
            string caption = woormTable.Columns[nColIdx].LocalizedText;
            ushort colID = woormTable.Columns[nColIdx].InternalID;
            
            string varType = woorm.RdeReader.GetVariableTypeFromId(colID);

            if (varType.CompareNoCase("DataEnum") || varType.CompareNoCase("Boolean") || varType.CompareNoCase("DateTime"))
                varType = "String";
 
            string st = "System." + varType;

            System.Type typ = System.Type.GetType(st);
            if (typ == null)
            {
                ColumnsType[nColIdx] = null;
                ColumnsID[nColIdx] = 0; DataColumnsIdx[nColIdx] = -1;
                return false;
            }

			if (!string.IsNullOrWhiteSpace(caption))
			{
				caption = caption.Replace('\r', ' ');
				caption = caption.Replace('\n', ' ');
				while (dataTable.Columns.Contains(caption))
				{
					caption += '.';
				}
			}
           
            
            DataColumn col = dataTable.Columns.Add(caption, typ);
            if (col == null)
            {
                ColumnsType[nColIdx] = null;
                ColumnsID[nColIdx] = 0; DataColumnsIdx[nColIdx] = -1;
                return false;
            }
            if (typ ==  System.Type.GetType("System.DateTime"))
                col.DateTimeMode = DataSetDateTime.Local;
            ColumnsID[nColIdx] = colID; DataColumnsIdx[nColIdx] = (short) (dataTable.Columns.Count - 1);

           Variable v = woorm.RdeReader.RdeSymbolTable.FindById(colID);
           if (v != null && v.WoormType != null)
            {
                string wt = v.WoormType;
                ColumnsType[nColIdx] = wt;
            }
           else
               ColumnsType[nColIdx] = string.Empty;

            return true;
        }

        //---------------------------------------------------------------------                    
        void NewRow()
        {
            DataRow r = dataTable.NewRow();
            dataTable.Rows.Add(r);
        }

        void AppendCellValue(int nColIdx, object value)
        {
            DataRow r = dataTable.Rows[dataTable.Rows.Count - 1];

            string s = (r[nColIdx] as string) + (value as string);

            r[nColIdx] = s;
        }

        //---------------------------------------------------------------------
        void AddCellValue (int nColIdx, object val)
        {
            if (val is DataEnum || val is bool || val is string)
                val = woorm.ReportSession.ApplicationFormatStyles.Format(val, null);
            
            DataRow r = dataTable.Rows[dataTable.Rows.Count - 1];
            short col = DataColumnsIdx[nColIdx];

            r[col] = val != null ? val : System.DBNull.Value;
        }

        bool GetCellData
                    (
                        int nRowIdx, int nColIdx,
                        out object sValue,
                        out bool isTailMultiLine
                    )
        {
            isTailMultiLine = woormTable.Columns[nColIdx].Cells[nRowIdx].Value.CellTail;

            sValue = woormTable.Columns[nColIdx].Cells[nRowIdx].Value.RDEData;

            return true;
        }
    }
}
