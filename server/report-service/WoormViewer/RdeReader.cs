using System;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Net;
using System.Linq;

using Microarea.Common.Applications;
using Microarea.Common.CoreTypes;
using Microarea.RSWeb.WoormEngine;
using Microarea.RSWeb.Objects;

using Microarea.Common;
using System.Diagnostics;
using Microarea.Common.NameSolver;

namespace Microarea.RSWeb.WoormViewer
{
    /// <summary>
    /// Descrizione di riepilogo per reader.
    /// </summary>
    /// ================================================================================
    public class RdeReader
    {
        private WoormDocument woorm;
        //private XmlReader reader;
        private int pageNo = 1;
        private int totalPages = 1;
        private bool allPagesReady = false;

        public bool AllPagesReady
        {
            get { return allPagesReady; }
            set { allPagesReady = value; }
        }
        private string graphics = string.Empty;
        private string woormFile = string.Empty;


        // contains the symboltable of 'vars' zone of woorm report, used by viewer to calculate hide expressions
        // It can be used also to extend the change graphical object functionality 
        // depending on woorm values. Data contained are those evaluated at the end of the run of engine
        public SymbolTable SymbolTable = new SymbolTable();

        // theese two collections are used to correctly format, in Woorm viewer,
        // data type read from xml file (the first collection contains C# types, the second one
        // woorm types (those written in the wrm file in the variable declaration section)
        private NameValueCollection AliasTable = new NameValueCollection();
        private NameValueCollection WoormTypesAliasTable = new NameValueCollection();

        public int CurrentPage
        {
            get { return pageNo; }
            set { pageNo = value; }
        }
        public int TotalPages { get { return totalPages; } set { totalPages = value; } }
        public string Graphics { get { return graphics; } set { graphics = value; } }
        public string WoormFile { get { return woormFile; } set { woormFile = value; } }
        public SymbolTable RdeSymbolTable { get { return SymbolTable; } }
        //---------------------------------------------------------------------------
        public TbSession Session { get { return woorm.ReportSession; } }

        //---------------------------------------------------------------------------
        public string GetVariableTypeFromId(ushort id)
        {
            return AliasTable[id.ToString()];
        }

        //---------------------------------------------------------------------------
        public CultureInfo GetCollateCultureFromId(ushort id)
        {
            Variable v = SymbolTable.FindById(id);
            return v == null ? CultureInfo.InvariantCulture : v.CollateCulture;
        }

        //---------------------------------------------------------------------------
        private void SeekToPage(PageType page)
        {
            switch (page)
            {
                case PageType.First: pageNo = 1; break;
                case PageType.Prev: if (pageNo > 1) pageNo--; break;
                case PageType.Next:
                    {
                        pageNo++;
                        if (AllPagesReady && pageNo > TotalPages)
                            pageNo--;
                        break;
                    }
                case PageType.Last: while (PathFinder.PathFinderInstance.ExistFile(Filename)) { pageNo++; }; pageNo--; break;
                case PageType.Current: break;
            }
        }

        //---------------------------------------------------------------------------
        private string InfoFilename
        {
            get { return woorm.InfoFilename; }
        }

        //---------------------------------------------------------------------------
        private string TotPageFilename
        {
            get { return woorm.TotPageFilename; }
        }

        ///<summary>
        ///Ritorna true se la "pageNo-esima" pagina e' gia' stata scritta dal RdeWrite, e quindi e' 
        ///pronta per essere visualizzata
        /// </summary>
        //---------------------------------------------------------------------------
        public bool IsPageReady()
        {
            return PathFinder.PathFinderInstance.ExistFile(Filename);
        }

        //---------------------------------------------------------------------------
        private string Filename
        {
            get { return woorm.CurrentRdeFilename(pageNo); }
        }

        //---------------------------------------------------------------------------
        public bool IsPageReady(int page)
        {
            return PathFinder.PathFinderInstance.ExistFile(woorm.CurrentRdeFilename(page));
        }

        //------------------------------------------------------------------------------
        public RdeReader(WoormDocument woorm)
        {
            this.woorm = woorm;
            this.SymbolTable.AddAlias("Woorm", "Framework.TbWoormViewer.TbWoormViewer");
        }

        //---------------------------------------------------------------------------
        ///<summary>
        ///Analogia con woorm c++: Campi speciali che sono usati e valutati solo in visualizzazione e non
        ///sono quindi nel file rde (xml nel caso di Easylook).
        /// </summary>
        private void AddSpecialFields()
        {
            Variable
            v = new Variable(SpecialReportField.NAME.IS_PRINTING);
            v.Data = false;
            SymbolTable.Add(v);

            v = new Variable(SpecialReportField.NAME.IS_ARCHIVING);
            v.Data = false;
            SymbolTable.Add(v);

            v = new Variable(SpecialReportField.NAME.LAST_PAGE);
            v.Data = 0;
            SymbolTable.Add(v);

            v = new Variable(SpecialReportField.NAME.USE_DEFAULT_ATTRIBUTE);
            v.Data = null;
            SymbolTable.Add(v);
        }

        //------------------------------------------------------------------------------
        public bool LoadTotPage()
        {
            if (!PathFinder.PathFinderInstance.ExistFile(this.TotPageFilename))
                return false;

            XmlReader reader = null;
            Stream fs = null;
            try
            {
                fs = PathFinder.PathFinderInstance.GetStream(TotPageFilename, false);
                reader = XmlReader.Create(fs, new XmlReaderSettings() { /*DtdProcessing = DtdProcessing.Prohibit,*/ IgnoreWhitespace = true });
 
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == RdeWriterTokens.Element.TotalPages)
                        {
                            reader.MoveToAttribute(RdeWriterTokens.Attribute.Number);
                            TotalPages = XmlConvert.ToInt32(reader.Value);
                            AllPagesReady = true;
                            Variable v = SymbolTable.Find(SpecialReportField.NAME.LAST_PAGE);
                            if (v != null)
                                v.Data = TotalPages;
                            break;
                        }

                        //riporta il reader al nodo element.
                        reader.MoveToElement();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format(WoormViewerStrings.ErrorReadingFile, TotPageFilename), ex);
            }
            finally
            {
                if (reader != null) { reader.Dispose(); reader = null; }
                if (fs != null) { fs.Dispose(); fs = null; }
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            return true;
        }
        //------------------------------------------------------------------------------
        public void LoadInfo()
        {
            if (!PathFinder.PathFinderInstance.ExistFile(InfoFilename))
                return;

            //pulisco symbolTable e AliasTable
            AliasTable.Clear();
            SymbolTable.Clear();

            //Analogia con woorm c++: Campi speciali che sono usati e valutati solo in visualizzazione e non
            //sono quindi nel file rde (xml nel caso di Easylook).
            AddSpecialFields();

            XmlReader reader = null;
            Stream fs = null;
            try
            {
                fs = PathFinder.PathFinderInstance.GetStream(InfoFilename,false);
                reader = XmlReader.Create(fs, new XmlReaderSettings() { /*DtdProcessing = DtdProcessing.Prohibit,*/ IgnoreWhitespace = true });

                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        switch (reader.Name)
                        {
                            case RdeWriterTokens.Element.TotalPages:
                                {
                                    reader.MoveToAttribute(RdeWriterTokens.Attribute.Number);
                                    TotalPages = XmlConvert.ToInt32(reader.Value);
                                    AllPagesReady = true;
                                    Variable v = SymbolTable.Find(SpecialReportField.NAME.LAST_PAGE);
                                    if (v != null)
                                        v.Data = TotalPages;

                                    break;
                                }

                            case RdeWriterTokens.Element.Alias:
                                {
                                    reader.MoveToAttribute(RdeWriterTokens.Attribute.Number);
                                    string id = reader.Value;

                                    reader.MoveToAttribute(RdeWriterTokens.Attribute.Type);
                                    string dataType = reader.Value;
                                    string woormType = dataType;
                                    string baseType = string.Empty;

                                    // backwards compatibility: attribute could not exist in persisted reports
                                    if (reader.MoveToAttribute(RdeWriterTokens.Attribute.WoormType))
                                    {
                                        woormType = reader.Value;
                                        WoormTypesAliasTable.Add(id, reader.Value);
                                    }
                                    if (reader.MoveToAttribute(RdeWriterTokens.Attribute.BaseType))
                                    {
                                        baseType = reader.Value;
                                    }

                                    reader.MoveToAttribute(RdeWriterTokens.Attribute.Name);
                                    string name = reader.Value;

                                    reader.MoveToAttribute(RdeWriterTokens.Attribute.Value);
                                    object data = null;
                                    if (dataType == "DataArray")
                                    {
                                        string arrayValue = reader.Value;
                                        data = SoapTypes.FromSoapDataArray(arrayValue, baseType);
                                     }
                                    else
                                        data = SoapTypes.From(reader.Value, dataType);

                                    string strIsColumn = null;
                                    if (reader.MoveToAttribute(RdeWriterTokens.Attribute.IsColumn))
                                        strIsColumn = reader.Value;

                                    Variable v = new Variable(name, ushort.Parse(id), null, 0, data);
                                    v.WoormType = woormType;
                                    v.BaseType = baseType;

                                    if (strIsColumn != null && strIsColumn == "true")
                                        v.IsColumn2 = true;

                                    if (reader.MoveToAttribute(RdeWriterTokens.Attribute.Culture))
                                    {
                                        int lcid = int.Parse(reader.Value);
                                        v.CollateCulture = new System.Globalization.CultureInfo(reader.Value);  //TODO RSWEB CollateCulture (2)
                                    }

                                    AliasTable[id] = dataType;
                                    SymbolTable.Add(v);

                                    break;
                                }

                            case RdeWriterTokens.Element.Graphics:
                                {
                                    reader.MoveToAttribute(RdeWriterTokens.Attribute.Source);
                                    woormFile = reader.Value;
                                    graphics = reader.ReadContentAsString();
                                    break;
                                }

                        }
                        //riporta il reader al nodo element.
                        reader.MoveToElement();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format(WoormViewerStrings.ErrorReadingFile, Filename), ex);
            }
            finally
            {
                if (reader != null) { reader.Dispose(); reader = null; }
                if (fs != null) { fs.Dispose(); fs = null; }
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }

        //------------------------------------------------------------------------------
        private string FormatFromSoapData(string formatStyleName, ushort id, string data, CultureInfo collateCulture)
        {
            // if no formatter is specified, then choose the best fitting one based on:
            // the woorm data type first
            // the C# data type as second best solution (backwards compatibility
            if (formatStyleName == DefaultFormat.None)
            {
                formatStyleName = WoormTypesAliasTable[id.ToString()];
                if (formatStyleName == null)
                {
                    string dataType = GetVariableTypeFromId(id);
                    if (dataType == null)
                        return data;
                    formatStyleName = ObjectHelper.DefaultFormatStyleName(dataType);
                }
            }

            return woorm.FormatStyles.FormatFromSoapData(formatStyleName, data, woorm.Namespace, collateCulture);
        }

        // dice se per quel tipo di dati è ammesso overflow nel caso la cella sia piccola
        //------------------------------------------------------------------------------
        private bool overflow(string fontStyleName, ushort id)
        {
            string dataType = this.woorm.RdeReader.GetVariableTypeFromId(id);
            return ObjectHelper.AllowOverflow(dataType);
        }

        //------------------------------------------------------------------------------
        public void SetArray(XmlReader reader)
        {
            reader.MoveToAttribute(RdeWriterTokens.Attribute.ID);
            ushort id = XmlConvert.ToUInt16(reader.Value);

            string val = string.Empty;
            if (reader.MoveToAttribute(RdeWriterTokens.Attribute.Value))
                val = WebUtility.HtmlDecode(reader.Value);

            Variable f = SymbolTable.FindById(id);
            if (f == null)
                return;

            string baseType = f.BaseType;
            if (string.IsNullOrEmpty(baseType))
                return;
            DataArray ar = DataArray.ConvertFromString(val, baseType);
            if (ar == null)
                return;
            f.Data = ar;

            //DEBUG
            ar.Elements.ForEach(e => Debug.Write(e+" "));         
        }

        //------------------------------------------------------------------------------
        public void SetElement(XmlReader reader, CellType cellType)
{
            reader.MoveToAttribute(RdeWriterTokens.Attribute.ID);
            ushort id = XmlConvert.ToUInt16(reader.Value);

            string val = string.Empty;
            if (reader.MoveToAttribute(RdeWriterTokens.Attribute.Value))
                val = WebUtility.HtmlDecode(reader.Value);

            bool isCellTail = false;
            if (reader.MoveToAttribute(RdeWriterTokens.Attribute.CellTail))
            {
                isCellTail = (reader.Value != null && reader.Value == "true");
            }

            foreach (BaseObj obj in woorm.Objects)
            {
                if (obj is Table)
                {
                    Table t = obj as Table;
                    foreach (Column col in t.Columns)
                        if (col.InternalID == id)
                        {
                            AssignColumnValue(cellType, id, val, isCellTail, t, col);
                            break;
                        }

                    continue;
                }

                if (obj is FieldRect)
                {
                    if (obj.AnchorRepeaterID != 0)
                        continue;

                    FieldRect t = obj as FieldRect;
                    if (t.InternalID == id)
                        AssignFieldValue(cellType, id, val, isCellTail, t);

                    continue;
                }

                if (obj is Repeater)
                {
                    Repeater r = obj as Repeater;

                    FieldRect t = r.FindField(id);
                    if (t != null)
                        AssignFieldValue(cellType, id, val, isCellTail, t);

                    continue;
                }
            }
        }

        private void AssignColumnValue(CellType cellType, ushort id, string val, bool isCellTail, Table t, Column col)
        {
            switch (cellType)
            {
                 case CellType.Cell:
                    {
                        Cell cell = col.Cells[t.CurrentRow];

                        string fromType = GetVariableTypeFromId(id);
                        if (string.IsNullOrEmpty(fromType))
                        {
                            throw new Exception(string.Format(WoormViewerStrings.UnknownVariableType, id));
                        }
                        object rdeData = SoapTypes.From(val, fromType);
                        string formattedData = FormatFromSoapData(col.FormatStyleName, id, val, GetCollateCultureFromId(id));
                        cell.Value.AssignData
                            (
                                rdeData,
                                formattedData,
                                isCellTail
                            );
                        cell.SubTotal = false;
                        break;
                    }
                case CellType.SubTotal:
                    {
                        Cell cell = col.Cells[t.CurrentRow];
                        cell.Value.AssignData
                            (
                            SoapTypes.From(val, GetVariableTypeFromId(id)),
                            FormatFromSoapData(col.FormatStyleName, id, val, GetCollateCultureFromId(id)),
                            isCellTail
                            );
                        cell.SubTotal = true;
                        break;
                    }
                case CellType.Total:
                    {
                        TotalCell cell = col.TotalCell;
                        cell.Value.AssignData
                            (
                            SoapTypes.From(val, GetVariableTypeFromId(id)),
                            FormatFromSoapData(col.FormatStyleName, id, val, GetCollateCultureFromId(id)),
                            isCellTail
                            );
                        break;
                    }
                case CellType.LowerInput:
                    {
                        Cell cell = col.Cells[t.CurrentRow];
                        cell.Value.AssignData
                            (
                            SoapTypes.From(val, GetVariableTypeFromId(id)),
                            CoreTypeStrings.Primo,
                            isCellTail
                            );
                        break;
                    }
                case CellType.UpperInput:
                    {
                        Cell cell = col.Cells[t.CurrentRow];
                        cell.Value.AssignData
                            (
                            SoapTypes.From(val, GetVariableTypeFromId(id)),
                            CoreTypeStrings.Ultimo,
                            isCellTail
                            );
                        break;
                    }
            }
        }

        private void AssignFieldValue(CellType cellType, ushort id, string val, bool isCellTail, FieldRect t)
        {
            switch (cellType)
            {
                case CellType.Cell:
                    {
                        t.Value.AssignData
                            (
                            SoapTypes.From(val, GetVariableTypeFromId(id)),
                            FormatFromSoapData(t.FormatStyleName, id, val, GetCollateCultureFromId(id)),
                            isCellTail
                            );
                        break;
                    }
                case CellType.LowerInput:
                    {
                        t.Value.AssignData
                            (
                            SoapTypes.From(val, GetVariableTypeFromId(id)),
                            CoreTypeStrings.Primo,
                            isCellTail
                            );
                        break;
                    }
                case CellType.UpperInput:
                    {
                        t.Value.AssignData
                            (
                            SoapTypes.From(val, GetVariableTypeFromId(id)),
                            CoreTypeStrings.Ultimo,
                            isCellTail
                            );
                        break;
                    }
            }
        }


        //------------------------------------------------------------------------------
        public BaseObj GetTableRepeater(XmlReader reader)
        {
            reader.MoveToAttribute(RdeWriterTokens.Attribute.ID);
            ushort id = XmlConvert.ToUInt16(reader.Value);

            BaseObj b = woorm.Objects.FindBaseObj(id);

            if (b != null && (b is Table || b is Repeater))
                return b;

            return null;
        }

        //------------------------------------------------------------------------------
        public void NextLine(XmlReader reader)
        {
            BaseObj t = GetTableRepeater(reader);
            if (t != null)
            {
                if (t is Table)
                    ((Table)t).CurrentRow++;
                else if (t is Repeater)
                    ((Repeater)t).CurrentRow++;
            }
        }

        //------------------------------------------------------------------------------
        public void Message(XmlReader reader)
        {
            reader.MoveToAttribute(RdeWriterTokens.Attribute.Message);
            woorm.Messages.Add(reader.Value);
        }

        //------------------------------------------------------------------------------
        public void Interline(XmlReader reader)
        {
            Table t = GetTableRepeater(reader) as Table;
            if (t != null && t.CurrentRow > 0)
                t.Interlines[t.CurrentRow - 1] = true;
        }
        public void TitleLine(XmlReader reader)
        {
            Table t = GetTableRepeater(reader) as Table;
            if (t != null)
                t.RowWithTitles[t.CurrentRow++] = true;
        }
        public void SubTitleLine(XmlReader reader)
        {
            Table t = GetTableRepeater(reader) as Table;
            if (t == null)
                return;

            string val = string.Empty;
            if (reader.MoveToAttribute(RdeWriterTokens.Attribute.Value))
                val = WebUtility.HtmlDecode(reader.Value);
            else
                return;

            t.RowWithCustomTitle[t.CurrentRow++] = val;
        }

        //------------------------------------------------------------------------------
        public void LoadPage(int pageNumber)
        {
            CurrentPage = pageNumber;

            LoadPage();
        }
        //------------------------------------------------------------------------------
        public void LoadPage(PageType page)
        {
            SeekToPage(page);

            LoadPage();
        }

        //------------------------------------------------------------------------------
        public void LoadPage()
        {
            woorm.NewPage();

            Variable v = this.SymbolTable.Find(SpecialReportField.NAME.PAGE);
            if (v != null)
            {
                v.Data = this.CurrentPage;
            }

            //"resetto" i subtotal, per evitare che rimangano da pag. precedente.
            foreach (BaseObj obj in woorm.Objects)
            {
                if (obj is Table)
                {
                    Table t = obj as Table;
                    foreach (Column col in t.Columns)
                    {
                        for (int i = 0; i < t.RowNumber; i++)
                        {
                            Cell cell = col.Cells[i];
                            cell.Clear();
                        }
                    }
                }
            }

            woorm.CurrentPage = pageNo;

            if (!PathFinder.PathFinderInstance.ExistFile(Filename))
            {
                LoadTotPage();
                return;
            }

            try
            {
                //Controllo se il file e' pronto per essere letto (in report che impiegano poco a eseguirsi (es.con solo un campo singolo))
                //a volte risulta che il writer non ha ancora rilasciato il file
                if (this.woorm.ReportSession.EngineType == EngineType.Paginated_Standard)
                {
                    using (Stream fs1 = PathFinder.PathFinderInstance.GetStream(Filename, true))
                    {
                        fs1.Dispose();
                    }
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                }

                // The file is not locked

                XmlReader reader = null;
                Stream fs = null;
                try
                {
                    fs = PathFinder.PathFinderInstance.GetStream(Filename, false);
                    reader = XmlReader.Create(fs, new XmlReaderSettings() { /*DtdProcessing = DtdProcessing.Prohibit,*/ IgnoreWhitespace = true });

                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            switch (reader.Name)
                            {
                                case RdeWriterTokens.Element.Cell: SetElement(reader, CellType.Cell); break;
                                case RdeWriterTokens.Element.SubTotal: SetElement(reader, CellType.SubTotal); break;
                                case RdeWriterTokens.Element.Total: SetElement(reader, CellType.Total); break;
                                case RdeWriterTokens.Element.LowerInput: SetElement(reader, CellType.LowerInput); break;
                                case RdeWriterTokens.Element.UpperInput: SetElement(reader, CellType.UpperInput); break;

                                case RdeWriterTokens.Element.NextLine: NextLine(reader); break;
                                case RdeWriterTokens.Element.SpaceLine: NextLine(reader); break;
                                case RdeWriterTokens.Element.Interline: Interline(reader); break;
                                case RdeWriterTokens.Element.TitleLine: TitleLine(reader); break;
                                case RdeWriterTokens.Element.SubTitleLine: SubTitleLine(reader); break;


                                case RdeWriterTokens.Element.NewPage: woorm.NewPage(); break;
                                case RdeWriterTokens.Element.Message: Message(reader); break;

                                case RdeWriterTokens.Element.Report: ApplyLayout(reader); break;

                                case RdeWriterTokens.Element.Array: SetArray(reader); break;
                            }

                            //riporta il reader al nodo element.
                            reader.MoveToElement();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format(WoormViewerStrings.ErrorReadingFile, Filename), ex);
                }
                finally
                {
                    if (reader != null) { reader.Dispose(); reader = null; }
                    if (fs != null) { fs.Dispose(); fs = null; }
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                }

                //guarda se e' pronto il file con scritto il numero definitivo di pagine
                LoadTotPage();
            }
            catch (Exception)
            {
                return;
            }
        }

        ///<summary>
        ///Legge l'attributo ReportLayout che e' in cima al file RDE
        ///e.g.
        ///<Report Release="1" ReportLayout="Default">
        ///e chiama il metodo del document che applica il layout
        ///</summary>
        //------------------------------------------------------------------------------
        private void ApplyLayout(XmlReader reader)
        {
            reader.MoveToAttribute(SpecialReportField.NAME.LAYOUT);
            string layout = (string)reader.Value;
            woorm.ApplyLayout(layout);
        }
    }
}
