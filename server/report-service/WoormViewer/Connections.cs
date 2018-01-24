using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;

using Microarea.Common.CoreTypes;
using Microarea.Common.Generic;
using Microarea.Common.Applications;
using Microarea.Common.Lexan;
using Microarea.Common.ExpressionManager;
using Microarea.RSWeb.WoormEngine;
using Microarea.RSWeb.WoormWebControl;
using TaskBuilderNetCore.Interfaces;
using System.Net;
using Microarea.Common;
using Microarea.Common.NameSolver;

namespace Microarea.RSWeb.WoormViewer
{
    public enum ConnectionLinkType { Report, Form, Function, URL, Radar, FormByAlias, ReportByAlias, FunctionByAlias, URLByAlias, Empty }
    public enum ConnectionLinkSubType { File = 0, Url = 1, MailTo = 2, CallTo = 3, GoogleMap = 4 };

    //================================================================================
    public class ConnectionLinkItem
    {
        string columnName;
        ushort alias;
        string dataType;
        string woormType;
        object data;
        ushort enumTag;

        Expression fieldValueExpr;

        //------------------------------------------------------------------------------------
        public Expression FieldValueExpr { get { return fieldValueExpr; } set { fieldValueExpr = value; } }
        public string ColumnName { get { return columnName; } }
        public ushort Alias { get { return alias; } }
        public string DataType { get { return dataType; } }
        public string WoormType { get { return woormType; } }
        public object Data { get { return data; } }
        public ushort EnumTag { get { return enumTag; } }

        //------------------------------------------------------------------------------
        public ConnectionLinkItem
            (
            string columnName,
            ushort alias,
            string dataType,
            string woormType,
            object data,
            ushort enumTag,
            Expression fieldValueExpr
            )

        {
            this.columnName = columnName;
            this.alias = alias;
            this.dataType = dataType;
            this.data = data;
            this.enumTag = enumTag;
            this.fieldValueExpr = fieldValueExpr;
            this.woormType = woormType;
        }

        //------------------------------------------------------------------------------
        public void Assign(ConnectionLinkItem source)
        {
            this.columnName = source.ColumnName;
            this.alias = source.Alias;
            this.dataType = source.DataType;
            this.data = source.Data == null ? ObjectHelper.Clone(source.Data) : null;
            this.enumTag = source.enumTag;
            this.fieldValueExpr = source.fieldValueExpr;
        }
    }

    //================================================================================
    public class ConnectionLinkFilter
    {
        private ConnectionLink connection;
        private string type = ConnectionLink.DEFAULT_TYPE;  //tipo di filtro
        private Token anOperator = Token.NOTOKEN;       //token dell'operatore 
        private ushort alias;                           //alias del valore da confrontare per abilitare il link
        private ushort dataAlias;                       //alias del valore da confrontare con il precedente
        private object data = null;                 //valore costante del filtro
        private ushort enumTag = 0;

        public string Type { get { return type; } set { type = value; } }
        public Token Operator { get { return anOperator; } set { anOperator = value; } }
        public ushort Alias { get { return alias; } set { alias = value; } }
        public ushort DataAlias { get { return dataAlias; } set { dataAlias = value; } }
        public object Data { get { return data; } set { data = value; } }
        public ushort EnumTag { get { return enumTag; } set { enumTag = value; } }

        //------------------------------------------------------------------------------
        public ConnectionLinkFilter(ConnectionLink connection)
        {
            this.connection = connection;
        }

        //-----------------------------------------------------------------------------------
        public bool Eval(WoormDocument woorm, int row)
        {
            bool whenOk = false;
            bool found, tail;

            object data1, data2;

            if (Alias == 0)
                return true; //filter inactive

            data1 = woorm.GetRDEData(Alias, row, out found, out tail);

            if (DataAlias != 0)
            {
                data2 = woorm.GetRDEData(DataAlias, row, out found, out tail);
            }
            else if (Data != null)
            {
                data2 = Data;
            }
            else
            {
                return false;
            }

            switch (Operator)
            {
                case Token.EQ:
                    whenOk = ObjectHelper.IsEquals(data1, data2);
                    break;
                case Token.NE:
                case Token.DIFF:
                    whenOk = !ObjectHelper.IsEquals(data1, data2);
                    break;
                case Token.GT:
                    whenOk = ObjectHelper.IsGreater(data1, data2);
                    break;
                case Token.GE:
                    whenOk = ObjectHelper.IsEquals(data1, data2) || ObjectHelper.IsGreater(data1, data2);
                    break;
                case Token.LT:
                    whenOk = ObjectHelper.IsLess(data1, data2);
                    break;
                case Token.LE:
                    whenOk = ObjectHelper.IsEquals(data1, data2) || ObjectHelper.IsLess(data1, data2);
                    break;
                case Token.LIKE:
                    {
                        string filter = ObjectHelper.CastString(data1);
                        string valueFilter = ObjectHelper.CastString(data2);

                        whenOk = filter.ToLower().StartsWith(valueFilter.ToLower());
                        break;
                    }
                default:
                    return false;
            }
            return whenOk;
        }
    }


    //================================================================================
    public class ConnectionLink : ArrayList
    {
        public const string DEFAULT_TYPE = "String";

        private ConnectionLinks connections;            // parent owner
        private ConnectionLinkType connectionType = ConnectionLinkType.Empty;
        private ConnectionLinkSubType connectionSubType = ConnectionLinkSubType.File; //inizializzato come in c++;

        private string tableName;           // Table name per radar, namespace per Report e Form
        private string strNameIdent;		//ON <ident>

        private ushort documentAlias = 0;   // Indica quale elemento contiene il vero Namespace (serve in Auditing)
        private ushort onAlias = 1;
        private Token filterOperator = Token.NOTOKEN;
        private bool valid = true;
        private bool syntaxWithExpression = false;

        private ConnectionLinkFilter filter1;
        private ConnectionLinkFilter filter2;
        private Expression filterExpr;

        private SymbolTable globalSymTable;
        private SymbolTable localSymTable;
        private string linkRemark;

        //private CommandBlock blockBefore;
        //private CommandBlock blockAfter;

        //------------------------------------------------------------------------------
        public TbSession Session { get { return connections.Session; } }
        public bool Valid { get { return valid; } }
        public ushort OnAlias { get { return onAlias; } }
        public string Namespace { get { return tableName; } }
        public ushort DocumentAlias { get { return documentAlias; } }
        public ConnectionLinkType ConnectionType { get { return connectionType; } }
        public ConnectionLinkSubType ConnectionSubType { get { return connectionSubType; } }
        public SymbolTable SymbolTable { get { return localSymTable; } }

        //------------------------------------------------------------------------------
        public ConnectionLink(ConnectionLinks connections, SymbolTable symTable)
            : base()
        {
            this.connections = connections;

            globalSymTable = symTable;
            localSymTable = new SymbolTable(globalSymTable);

            //localSymTable.Add(new Field());

            filter1 = new ConnectionLinkFilter(this);
            filter2 = new ConnectionLinkFilter(this);
        }

        //------------------------------------------------------------------------------
        /// <summary>
        /// Ritorna il fragment xml che descrive gli argomenti da passare al report invocato
        /// </summary>
        /// <param name="woorm"></param>
        /// <param name="atRowNumber">indice della riga di tabella che contiene il link (usato per calcolare la chiave per trovare i campi gia` visitati)</param>
        /// <param name="sourceValue">il valore del campo che origina il link (usato per calcolare la chiave per trovare i campi gia` visitati)</param>
        /// <returns></returns>
        public string GetArgumentsOuterXml(WoormDocument woorm, int atRowNumber, string sourceValue = null)
        {
            bool found;
            XmlDocument d = new XmlDocument();
            d.AppendChild(d.CreateElement(WebMethodsXML.Element.Arguments));
            if (atRowNumber > -1)
                d.DocumentElement.SetAttribute(WebMethodsXML.Attribute.Row, atRowNumber.ToString());
            if (sourceValue != null)    //potrebbe non servire piu'
                d.DocumentElement.SetAttribute(WebMethodsXML.Attribute.Value, sourceValue);
            FunctionPrototype fi = new FunctionPrototype();

            //Necessaria per i link sui campi singoli
            woorm.SynchronizeSymbolTable(atRowNumber);

            foreach (ConnectionLinkItem item in this)
            {
                Parameter pInfo = new Parameter(item.ColumnName, item.DataType);

                if (item.FieldValueExpr != null)
                {
                    Value v = item.FieldValueExpr.Eval();
                    pInfo.ValueString = v != null ? SoapTypes.To(v.Data) : "";
                }
                else if (item.Data != null)
                    pInfo.ValueString = SoapTypes.To(item.Data);
                else
                {
                    bool tail;
                    object dataObject = woorm.GetRDEData(item.Alias, atRowNumber, out found, out tail);

                    pInfo.ValueString = dataObject == null ? "" : SoapTypes.To(dataObject);
                }

                fi.Parameters.Add(pInfo);
            }

            fi.Parameters.Unparse(d.DocumentElement);
            return d.OuterXml;
            //		<Arguments row="1">
            //		<Param name="w_IsJournal" type="Bool" mode="in" localize="First Argument" value="0" />
            //		</Arguments>
        }


        //------------------------------------------------------------------------------
        public string GetHttpGetRequest(string connectionValue, int atRowNumber)
        {
            //if (
            //	(connectionType != ConnectionLinkType.URL)
            //	&&
            //	(connectionType != ConnectionLinkType.URLByAlias)
            //             &&
            //             (connectionSubType != ConnectionLinkSubType.Url)
            //             )
            //	return string.Empty;
            if (connectionValue.IsNullOrEmpty())
                return string.Empty;

            string paramName = string.Empty, paramValue = string.Empty;

            int numParam = 0;

            StringBuilder sbLinkUrl = new StringBuilder();
            sbLinkUrl.Append(connectionValue);
            if (this.Count > 0)
                sbLinkUrl.Append('?');

            foreach (ConnectionLinkItem item in this)
            {
                if (numParam > 0)
                    sbLinkUrl.Append('&');

                numParam++;
                paramName = item.ColumnName;
                if (item.FieldValueExpr != null)
                {
                    Value v = item.FieldValueExpr.Eval();
                    paramValue = v.Data.ToString();
                }
                else if (item.Data != null)
                    paramValue = item.Data.ToString();

                sbLinkUrl.Append(paramName);
                sbLinkUrl.Append('=');
                sbLinkUrl.Append(paramValue);
            }

            return sbLinkUrl.ToString(); //es. http://www.sitoweb.it/search?param1=value1&param2=value2
        }

        //------------------------------------------------------------------------------
        private bool IsAlpha(string val)
        {
            foreach (char ch in val)
                if (!char.IsDigit(ch))
                    return true;
            return false;
        }

        //------------------------------------------------------------------------------
        bool ParseField(WoormParser lex)
        {
            string name = string.Empty;
            string dataType = DEFAULT_TYPE;
            string woormType = "";
            ushort enumTag = 0;

            do
            {
                dataType = "String";
                if (lex.LookAhead(Token.END))
                    return true;

                if (
                    (connectionType != ConnectionLinkType.Radar) &&
                    !DataTypeParser.Parse(lex, Session.Enums, out dataType, out woormType, out enumTag)
                    )
                    return false;

                switch (lex.LookAhead())
                {
                    case Token.ID:
                        {
                            if (!lex.ParseID(out name))
                                return false;

                            if (lex.LookAhead(Token.ALIAS))
                            {
                                documentAlias = 0;
                                if (!ParseAlias(lex, out documentAlias))
                                    return false;

                                Add(new ConnectionLinkItem(name, documentAlias, dataType, woormType, null, enumTag, null));
                            }
                            else if (lex.Matched(Token.ASSIGN)) //non yet supported, this is only an experiment!
                            {
                                Expression exp = new Expression(connections.Document.ReportSession, connections.Document.SymbolTable);

                                exp.StopTokens = new StopTokens(new Token[] { Token.SEP });
                                exp.ForceSkipTypeChecking = connections.Document.ForLocalizer;
                                if (!exp.Compile(lex, CheckResultType.Compatible, dataType))
                                {
                                    lex.SetError(WoormViewerStrings.BadExpression);
                                    return false;
                                }
                                lex.SkipToken();

                                Add(new ConnectionLinkItem(name, documentAlias, dataType, woormType, null, enumTag, exp));
                            }
                            else
                            {
                                object data = null;
                                if (!ParseConstValue(lex, dataType, out data))
                                    return false;

                                Add(new ConnectionLinkItem(name, 0, dataType, woormType, data, enumTag, null));
                            }

                            break;
                        }

                    case Token.EOF:
                        lex.SetError(WoormViewerStrings.UnexpectedEOF);
                        return false;

                    case Token.END:
                        return true;

                    case Token.LINKFUNCTION:
                    case Token.LINKFORM:
                    case Token.LINKREPORT:
                    case Token.LINKRADAR:
                    case Token.LINKURL:
                        lex.SetError(WoormViewerStrings.MissedEnd);
                        return false;

                    default:
                        lex.SetError(WoormViewerStrings.MissedEnd);
                        return false;
                }
            }
            while (true);
        }

        //------------------------------------------------------------------------------
        bool ParseFields(WoormParser lex)
        {
            bool ok = true;

            do { ok = ParseField(lex) && !lex.Error && !lex.Eof; }
            while (ok && !lex.LookAhead(Token.END));

            return ok;
        }

        //------------------------------------------------------------------------------
        bool ParseConstValue(WoormParser lex, string type, out object data)
        {
            data = ObjectHelper.CreateObject(type);

            switch (type)
            {
                case "Int32":
                    {
                        int n = 0;
                        if (!lex.ParseSignedInt(out n))
                            return false;

                        ObjectHelper.Assign(ref data, n);
                        break;
                    }
                case "Int64":
                    {
                        long l = 0;
                        if (!lex.ParseSignedLong(out l))
                            return false;

                        ObjectHelper.Assign(ref data, l);
                        break;
                    }
                case "Boolean":
                    {
                        bool b = false;
                        if (!lex.ParseBool(out b))
                            return false;

                        ObjectHelper.Assign(ref data, b);
                        break;
                    }
                case "String":
                    {
                        string s;
                        if (!lex.ParseString(out s))
                            return false;

                        s = s.Replace("%", ""); // elimina il % di un eventuale like
                        ObjectHelper.Assign(ref data, s);
                        break;
                    }
                case "Single":
                case "Double":
                    {
                        double f = 0;
                        if (!lex.ParseSignedDouble(out f))
                            return false;

                        ObjectHelper.Assign(ref data, f);
                        break;
                    }
                case "DateTime":
                case "DataEnum":
                    {
                        object anydata;
                        if (!ComplexDataParser.Parse(lex, Session.Enums, out anydata))
                            return false;

                        ObjectHelper.Assign(ref data, anydata);
                        break;
                    }
                default:
                    return false;
            }
            return true;
        }

        //------------------------------------------------------------------------------
        bool ParseFilterClause(WoormParser lex, ref ConnectionLinkFilter filter)
        {
            string type = DEFAULT_TYPE;
            string woormType = string.Empty;
            string tagName = string.Empty;
            ushort alias = 0;
            ushort enumTag = 0;

            if (syntaxWithExpression)
            {
                filterExpr = new Expression(connections.Document.ReportSession, connections.Document.SymbolTable);
                filterExpr.StopTokens = new StopTokens(new Token[] { Token.BEGIN });
                filterExpr.ForceSkipTypeChecking = connections.Document.ForLocalizer;
                if (!filterExpr.Compile(lex, CheckResultType.Match, "Boolean"))
                {
                    lex.SetError(WoormViewerStrings.BadExpression);
                    return false;
                }
            }
            else //syntax without expression
            {
                if (!(
                    DataTypeParser.Parse(lex, Session.Enums, out type, out woormType, out enumTag, out tagName) &&
                    ParseAlias(lex, out alias)
                    ))
                    return false;

                filter.Type = type;
                filter.Alias = alias;
                filter.EnumTag = enumTag;
                filter.Operator = lex.LookAhead();

                if
                    (!(
                    filter.Operator == Token.EQ ||
                    filter.Operator == Token.NE ||
                    filter.Operator == Token.LT ||
                    filter.Operator == Token.LE ||
                    filter.Operator == Token.GT ||
                    filter.Operator == Token.GE ||
                    filter.Operator == Token.LIKE ||
                    filter.Operator == Token.DIFF
                    ))
                {
                    lex.SetError(WoormViewerStrings.InvalidFilterOperator);
                    return false;
                }

                if (filter.Operator == Token.LIKE && filter.Type != "String")
                {
                    lex.SetError(WoormViewerStrings.LikeTypeNotAllowed);
                    return false;
                }

                if
                    (
                    filter.Operator != Token.EQ &&
                    filter.Operator != Token.NE &&
                    filter.Operator != Token.DIFF &&
                    filter.Type == "Boolean"
                    )
                {
                    lex.SetError(WoormViewerStrings.InvalidBooleanFilterOperator);
                    return false;
                }

                lex.SkipToken();

                if (lex.LookAhead() == Token.ALIAS)
                {
                    alias = 0;
                    if (!ParseAlias(lex, out alias))
                        return false;

                    filter.DataAlias = alias;
                }
                else
                {
                    object data = null;

                    if (!ParseConstValue(lex, type, out data)) return false;
                    filter.Data = data;
                }
            }

            return true;
        }

        //-----------------------------------------------------------------------------------
        public bool EvalFilters(WoormDocument woorm, int row)
        {
            if (syntaxWithExpression)
            {
                if (filterExpr != null)          //presente clausola when
                {
                    //gia fatto woorm.SynchronizeSymbolTable(row);
                    Value v = filterExpr.Eval();
                    if (v != null && v.Data != null && v.Valid)
                        return (bool)v.Data;
                    else
                        return false;
                }
                else                    //non presente clausola when, quindi connessione sempre valida
                    return true;
            }
            else
            {
                if (filter1.Eval(woorm, row))
                {
                    if (filterOperator == Token.OR || filterOperator == Token.NOTOKEN)
                        return true;
                    if (filterOperator == Token.AND)
                        return filter2.Eval(woorm, row);
                }
                else
                {
                    if (filterOperator == Token.OR)
                        return filter2.Eval(woorm, row);
                    if (filterOperator == Token.AND || filterOperator == Token.NOTOKEN)
                        return false;
                }
            }

            return false;
        }


        // potrebbe avere un alias di colonna allora è quello buono (quello di tabella lo skippo)
        //-----------------------------------------------------------------------------
        private bool ParseAlias(WoormParser lex, out ushort alias)
        {
            alias = 0;

            return (lex.ParseTag(Token.ALIAS) && lex.ParseAlias(out alias));
        }

        //------------------------------------------------------------------------------
        public bool Parse(WoormParser lex)
        {
            if (lex.Matched(Token.LINKFUNCTION)) connectionType = ConnectionLinkType.Function;
            else if (lex.Matched(Token.LINKFORM)) connectionType = ConnectionLinkType.Form;
            else if (lex.Matched(Token.LINKREPORT)) connectionType = ConnectionLinkType.Report;
            else if (lex.Matched(Token.LINKRADAR)) connectionType = ConnectionLinkType.Radar;
            else if (lex.Matched(Token.LINKURL))
            {
                connectionType = ConnectionLinkType.URL;
                connectionSubType = ConnectionLinkSubType.Url;

                if (lex.NextTokenIsInt)
                {
                    int sub = 0;
                    lex.ParseInt(out sub);
                    switch ((ConnectionLinkSubType)sub)
                    {
                        case ConnectionLinkSubType.Url:
                            connectionSubType = ConnectionLinkSubType.Url;
                            break;
                        case ConnectionLinkSubType.File:
                            connectionSubType = ConnectionLinkSubType.File;
                            break;
                        case ConnectionLinkSubType.MailTo:
                            connectionSubType = ConnectionLinkSubType.MailTo;
                            break;
                        case ConnectionLinkSubType.CallTo:
                            connectionSubType = ConnectionLinkSubType.CallTo;
                            break;
                        case ConnectionLinkSubType.GoogleMap:
                            connectionSubType = ConnectionLinkSubType.GoogleMap;
                            break;

                        default:
                            break;
                    }
                }
            }

            if (connectionType == ConnectionLinkType.Radar)
            {
                if (!lex.ParseID(out tableName))
                    return false;
            }
            else if (lex.LookAhead(Token.ALIAS))  // es. LinkReport alias 51
            {
                if (!ParseAlias(lex, out documentAlias))
                    return false;
                // lookup id --->strName
                Variable var = connections.Document.SymbolTable.FindById(documentAlias);
                if (var != null)
                    tableName = var.Name;

                switch (connectionType)
                {
                    case ConnectionLinkType.Form:
                        {
                            connectionType = ConnectionLinkType.FormByAlias;
                            break;
                        }
                    case ConnectionLinkType.Report:
                        {
                            connectionType = ConnectionLinkType.ReportByAlias;
                            break;
                        }
                    case ConnectionLinkType.Function:
                        {
                            connectionType = ConnectionLinkType.FunctionByAlias;
                            break;
                        }
                    case ConnectionLinkType.URL:
                        {
                            connectionType = ConnectionLinkType.URLByAlias;
                            break;
                        }
                }
            }
            else if (lex.LookAhead(Token.ID)) // es. LinkReport w_nomeReport
            {
                if (!lex.ParseID(out tableName))
                    return false;
                Variable var = connections.Document.SymbolTable.Find(tableName);
                if (var != null)
                    documentAlias = var.Id;

                switch (connectionType)
                {
                    case ConnectionLinkType.Form:
                        {
                            connectionType = ConnectionLinkType.FormByAlias;
                            break;
                        }
                    case ConnectionLinkType.Report:
                        {
                            connectionType = ConnectionLinkType.ReportByAlias;
                            break;
                        }
                    case ConnectionLinkType.Function:
                        {
                            connectionType = ConnectionLinkType.FunctionByAlias;
                            break;
                        }
                    case ConnectionLinkType.URL:
                        {
                            connectionType = ConnectionLinkType.URLByAlias;
                            break;
                        }
                }
            }
            else   // es. LinkReport "Report.account.nomeReportEsplicito"
            {
                // Form or Report namespace
                if (!lex.ParseString(out tableName))
                    return false;
            }

            lex.Matched(Token.ON);
            if (lex.LookAhead(Token.ALIAS))
            {
                if (!ParseAlias(lex, out onAlias))
                    return false;

                if (connectionType != ConnectionLinkType.Radar)
                {
                    Variable var = connections.Document.SymbolTable.FindById(onAlias);
                    if (var != null)
                        strNameIdent = var.Name;
                }
            }
            else //ID
            {
                if (!lex.ParseID(out strNameIdent))
                    return false;
                Variable var = connections.Document.SymbolTable.Find(strNameIdent);
                if (var != null)
                    onAlias = var.Id;
                else if (!connections.Document.ForLocalizer)
                {
                    lex.SetError(string.Format(ExpressionManagerStrings.UnknownField, strNameIdent));
                    return false;
                }
                syntaxWithExpression = true;
            }

            if (connectionType != ConnectionLinkType.Radar && lex.LookAhead(Token.WHEN))
            {
                lex.SkipToken();
                if (!ParseFilterClause(lex, ref filter1))
                    return false;

                if (lex.LookAhead(Token.AND) || lex.LookAhead(Token.OR))
                {
                    filterOperator = lex.LookAhead();
                    lex.SkipToken();
                    if (!ParseFilterClause(lex, ref filter2))
                        return false;
                }
            }

            // controlla che il namespace sia valido TODO per altri tipi
            if (connectionType == ConnectionLinkType.Form)
            {
                if (tableName.IndexOf(NameSpaceSegment.Document + ".") < 0)
                {
                    NameSpace ns = NameSpaceSegment.Document + "." + tableName;
                    valid = ns.IsValid();
                }
            }
            else if (connectionType == ConnectionLinkType.Report)
            {
                if (tableName.IndexOf(NameSpaceSegment.Report + ".") < 0)
                {
                    NameSpace ns = NameSpaceSegment.Report + "." + tableName;
                    valid = ns.IsValid();
                }
            }

            bool ok =
                lex.ParseBegin() &&
                ParseFields(lex) &&
                lex.ParseEnd();

            //---- TODO
            lex.DoAudit = true;

            if (lex.Matched(Token.CONTEXT))
            {
                lex.SkipBlock();
            }
            if (lex.Matched(Token.BEFORE))
            {
                lex.SkipBlock();
            }
            if (lex.Matched(Token.AFTER))
            {
                lex.SkipBlock();
            }

            linkRemark = lex.GetAuditString();
            //----

            return ok;
        }

        //------------------------------------------------------------------------------
        public string GetGoogleMapURL(string Address)
        {
            string Zip = "", Country = "", County = "", City = "";
            string Latitude = "", Longitude = "", FederalState = "", StreetNumber = "";

            for (int i = 0; i < Count; i++)
            {
                string paramValue = "";
                ConnectionLinkItem item = (ConnectionLinkItem)this[i];

                if (item.FieldValueExpr != null)
                {
                    Value v = item.FieldValueExpr.Eval();
                    paramValue = (v != null && v.Data != null) ? v.Data.ToString() : "";
                }

                if (string.Compare(item.ColumnName, "Country", true) == 0)
                    Country = paramValue;
                else if (string.Compare(item.ColumnName, "County", true) == 0)
                    County = paramValue;
                else if (string.Compare(item.ColumnName, "City", true) == 0)
                    City = paramValue;
                else if (string.Compare(item.ColumnName, "ZipCode", true) == 0)
                    Zip = paramValue;

                else if (string.Compare(item.ColumnName, "Address", true) == 0)
                    Address = paramValue;

                else if (string.Compare(item.ColumnName, "Latitude", true) == 0)
                    Latitude = paramValue;
                else if (string.Compare(item.ColumnName, "Longitude", true) == 0)
                    Longitude = paramValue;
                else if (string.Compare(item.ColumnName, "FederalState", true) == 0)
                    FederalState = paramValue;
                else if (string.Compare(item.ColumnName, "StreetNumber", true) == 0)
                    StreetNumber = paramValue;
            }

            string sG = string.Empty;
            Latitude.Trim(); Longitude.Trim();
            if (!Latitude.IsNullOrEmpty() && !Longitude.IsNullOrEmpty())
                sG = Latitude + ',' + Longitude;
            else
                sG = GoogleMapAddress(Address, StreetNumber, City, County, Country, FederalState, Zip);

            return string.Format("http://maps.google.com/maps?f=q&hl=en&t=m&q={0}", sG);
        }

        //----------------------------------------------------------------------------
        void AddEncodeComp(ref string completeAddress, string aComp)
        {
            aComp.Trim();
            if (!aComp.IsNullOrEmpty())
                if (completeAddress.IsNullOrEmpty())
                    completeAddress = WebUtility.HtmlEncode(aComp);
                else
                    completeAddress += ",+" + WebUtility.HtmlEncode(aComp);
        }

        //----------------------------------------------------------------------------
        private string GoogleMapAddress(string Address, string City, string County, string Country, string Zip, string streetNumber, string federalState)
        {
            string completAddress = string.Empty;

            AddEncodeComp(ref completAddress, Address);
            AddEncodeComp(ref completAddress, streetNumber);
            AddEncodeComp(ref completAddress, City);
            AddEncodeComp(ref completAddress, County);
            AddEncodeComp(ref completAddress, Country);
            AddEncodeComp(ref completAddress, federalState);
            AddEncodeComp(ref completAddress, Zip);

            return completAddress;
        }

        //----------------------------------------------------------------------------
        internal string GetNavigateUrl(int atRowNumber)
        {
            string connectionValue = Namespace;
            if (ConnectionType == ConnectionLinkType.URLByAlias)
            {
                Variable v = connections.Document.RdeReader.SymbolTable.Find(Namespace);
                if (v != null && v.Data != null)
                    connectionValue = v.Data.ToString();
            }
            if (connectionValue.IsNullOrEmpty())
                return "";
            try
            {
                switch (ConnectionSubType)
                {
                    case ConnectionLinkSubType.File:
                    case ConnectionLinkSubType.Url:
                        {
                            //se e' un www...gli metto http
                            if (connectionValue.Length > 3 && String.Compare(connectionValue.Substring(0, 3), "www", true) == 0)
                                connectionValue = "http://" + connectionValue;

                            //Provo a vedere se e' un namespace
                            NameSpace nslinkedFile = new NameSpace(connectionValue);
                            if (nslinkedFile.IsValid())
                                connectionValue = Session.PathFinder.GetFilename(nslinkedFile, String.Empty);

                            Uri linkUrl = new Uri(connectionValue);

                            if (linkUrl.IsFile)
                            {
                                //copia del file locale sotto la "webfolder esposta"
                                string sourceFilePath = connectionValue;
                                string ext = Path.GetExtension(sourceFilePath);
                                FileProvider fp = new FileProvider(this.connections.Document, ext);
                                string destPath = fp.GenericTmpFile;

                                if (PathFinder.PathFinderInstance.FileSystemManager.ExistFile(sourceFilePath))
                                {
                                    PathFinder.PathFinderInstance.FileSystemManager.CopyFile(sourceFilePath, destPath, true);
                                    return string.Format("~\\{0}", fp.GenericTmpFileRelPath);
                                }
                            }
                            else
                            {   //http...se ci sono parametri costruisco la stringa	
                                return GetHttpGetRequest(connectionValue, atRowNumber);
                            }
                            break;
                        }
                    case ConnectionLinkSubType.CallTo:
                        {
                            string prefix = "";
                            for (int i = 0; i < Count; i++)
                            {
                                string paramValue = "";
                                ConnectionLinkItem item = (ConnectionLinkItem)this[i];

                                if (item.FieldValueExpr != null)
                                {
                                    Value v = item.FieldValueExpr.Eval();
                                    paramValue = v.Data.ToString();
                                }

                                if (string.Compare(item.ColumnName, "TelephonePrefix", true) == 0)
                                    prefix = paramValue;
                            }
                            return string.Format("callto:{0}{1}", prefix, connectionValue);
                        }
                    case ConnectionLinkSubType.MailTo:
                        {
                            return string.Format("mailto:{0}", connectionValue);
                        }
                    case ConnectionLinkSubType.GoogleMap:
                        {
                            return GetGoogleMapURL(connectionValue);
                        }
                }
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
                return "";
            }
            return "";
        }

        //------------------------------------------------------------------------------------
        public bool Unparse(Unparser unparser)
        {

            switch (connectionType)
            {
                case ConnectionLinkType.Report:
                    {
                        unparser.WriteTag(Token.LINKREPORT, false);
                        unparser.WriteString(tableName, false); //m_strName = ns esplicito 
                        break;
                    }
                case ConnectionLinkType.Form:
                    {
                        unparser.WriteTag(Token.LINKFORM, false);
                        unparser.WriteString(tableName, false); //m_strName = ns esplicito
                        break;
                    }
                case ConnectionLinkType.ReportByAlias:
                    {
                        unparser.WriteTag(Token.LINKREPORT, false);
                        unparser.WriteID(tableName, false); //m_strName = nome variabile

                        //era già commentato in c++
                        //unparser.WriteTag(T_ALIAS, FALSE);
                        //unparser.UnparseInt(m_nNsDocumentAlias, FALSE);
                        break;
                    }
                case ConnectionLinkType.FormByAlias:
                    {
                        unparser.WriteTag(Token.LINKFORM, false);
                        unparser.WriteID(tableName, false);  //m_strName = nome variabile

                        //era già commentato in c++
                        //unparser.WriteTag(T_ALIAS, FALSE);
                        //unparser.UnparseInt(m_nNsDocumentAlias, FALSE);
                        break;
                    }
                case ConnectionLinkType.Radar:
                    {
                        unparser.WriteTag(Token.LINKRADAR, false);
                        unparser.WriteID(tableName, false);
                        break;
                    }
                case ConnectionLinkType.Function:
                    {
                        unparser.WriteTag(Token.LINKFUNCTION, false);
                        unparser.WriteString(tableName, false);          //m_strName = ns esplicito    
                        break;
                    }
                case ConnectionLinkType.FunctionByAlias:
                    {
                        unparser.WriteTag(Token.LINKFUNCTION, false);
                        unparser.WriteID(tableName, false);           //m_strName = nome variabile
                        break;
                    }
                case ConnectionLinkType.URL:
                    {
                        unparser.WriteTag(Token.LINKURL, false);

                        if (connectionSubType == ConnectionLinkSubType.MailTo)
                            unparser.Write((int)ConnectionLinkSubType.MailTo, false);
                        else if (connectionSubType == ConnectionLinkSubType.CallTo)
                            unparser.Write((int)ConnectionLinkSubType.CallTo, false);
                        else if (connectionSubType == ConnectionLinkSubType.GoogleMap)
                            unparser.Write((int)ConnectionLinkSubType.GoogleMap, false);

                        unparser.WriteBlank();

                        unparser.WriteString(tableName, false);          //m_strName = ns esplicito
                        break;
                    }
                case ConnectionLinkType.URLByAlias:
                    {
                        unparser.WriteTag(Token.LINKURL, false);

                        if (connectionSubType == ConnectionLinkSubType.MailTo)
                            unparser.Write((int)ConnectionLinkSubType.MailTo, false);
                        else if (connectionSubType == ConnectionLinkSubType.CallTo)
                            unparser.Write((int)ConnectionLinkSubType.CallTo, false);
                        else if (connectionSubType == ConnectionLinkSubType.GoogleMap)
                            unparser.Write((int)ConnectionLinkSubType.GoogleMap, false);

                        unparser.WriteBlank();

                        unparser.WriteID(tableName, false);           //m_strName = nome variabile
                        break;
                    }
                default:
                    break;
            }

            unparser.WriteLine();
            unparser.IncTab();

            unparser.WriteTag(Token.ON, false);

            if (connectionType == ConnectionLinkType.Radar)
            {
                unparser.WriteTag(Token.ALIAS, false);
                unparser.Write(onAlias, false);
            }
            else
                unparser.WriteID(strNameIdent, false);

            unparser.WriteLine();

            if (connectionType != ConnectionLinkType.Radar)
            {
                if (filterExpr != null && !filterExpr.IsEmpty)
                {
                    unparser.WriteTag(Token.WHEN, false);

                    unparser.IncTab();
                    WriteFilter(unparser);
                    unparser.DecTab();
                }
            }

            WriteItems(unparser);

            //TODOLUCA  non vengono neanche parsati
            //unparser.IncTab();
            //if (m_pBeforeLink && !m_pBeforeLink->IsEmpty())
            //{
            //    unparser.WriteTag(Token.BEFORE, false);
            //    m_pBeforeLink->Unparse(unparser);
            //}
            //if (m_pAfterLink && !m_pAfterLink->IsEmpty())
            //{
            //    unparser.WriteTag(Token.AFTER, false);
            //    m_pAfterLink->Unparse(unparser);
            //}
            //unparser.DecTab();
            unparser.Write(this.linkRemark);
            //----

            unparser.DecTab();

            return true;
        }

        //------------------------------------------------------------------------------------
        private void WriteItems(Unparser unparser)
        {
            unparser.WriteBegin();
            unparser.IncTab();

            foreach (ConnectionLinkItem item in this)
            {
                if (item.Alias == SpecialReportField.ID.LINKED_DOC)
                    continue;

                if (connectionType != ConnectionLinkType.Radar)
                {
                    unparser.UnparseDataType(item.WoormType, "", item.EnumTag, false, false); //DATATYPE::NULL ==  "NULL"?

                    if (item.WoormType == "Enum")
                        unparser.Write(" /* " + Session.Enums.TagName(item.EnumTag) + " */ ");

                    unparser.WriteID(item.ColumnName, false);

                    unparser.WriteTag(Token.ASSIGN, false);

                    if (item.FieldValueExpr != null)
                        unparser.WriteExpr(item.FieldValueExpr.ToString(), false);

                    unparser.WriteTag(Token.SEP, true);
                }
                else
                {
                    unparser.WriteID(item.ColumnName, false);
                    unparser.WriteTag(Token.ALIAS, false);
                    unparser.Write(item.Alias);
                }
            }
            unparser.DecTab();
            unparser.WriteEnd();
            unparser.WriteLine();
        }

        //------------------------------------------------------------------------------------
        private void WriteFilter(Unparser unparser, bool convertExpression = false)
        {
            if (convertExpression)
            {
                WriteFilterClause(unparser, filter1);

                if (filter2.Alias > 0)
                {
                    unparser.WriteBlank();
                    unparser.WriteTag(filterOperator, false);

                    WriteFilterClause(unparser, filter2);
                }
            }
            else
                unparser.WriteExpr(filterExpr.ToString(), true);
        }

        //------------------------------------------------------------------------------------
        private void WriteFilterClause(Unparser unparser, ConnectionLinkFilter filter)
        {
            ushort alias = 0;
            string ident = string.Empty;
            ushort.TryParse(filter.Alias.ToString(), out alias);
            Variable v = SymbolTable.FindById(alias);
            if (v != null)
                ident = v.Name;

            unparser.WriteID(ident, false);
            unparser.WriteBlank();
            unparser.WriteTag(filter.Operator, false);

            if (filter.Data != null)
                WriteConstValue(unparser, filter.Data);
            else
            {
                ushort.TryParse(filter.DataAlias.ToString(), out alias);
                Variable valueAlias = SymbolTable.FindById(alias);
                if (valueAlias != null)
                    ident = valueAlias.Name;
                unparser.WriteID(ident, false);
            }
        }

        //------------------------------------------------------------------------------------
        private void WriteConstValue(Unparser unparser, object val)
        {
            if (val == null)
                return;
            string valType = val.GetType().Name;
            switch (valType)
            {
                case "Int32":
                    unparser.Write((Int32)val, false);
                    break;
                case "Int64":
                    unparser.Write((Int64)val, false);
                    break;
                case "Boolean":
                    unparser.Write((bool)val, false);
                    break;
                case "String":
                    unparser.Write((string)val);
                    break;
                case "Single":
                case "Double":
                    unparser.Write((Double)val, false);
                    break;
                case "DateTime":
                    unparser.WriteDate((DateTime)val);
                    break;
                case "DataEnum":
                    //unparser.Write((DateTime)val, false);
                    break;
            }
        }
    }

    /// Gestione della connessione ad altri report o form
    /// </summary>
    /// ================================================================================
    public class ConnectionLinks : ArrayList
    {
        private WoormDocument document;

        //------------------------------------------------------------------------------------
        public WoormDocument Document { get { return document; } set { document = value; } }
        //------------------------------------------------------------------------------
        public TbReportSession Session { get { return document.ReportSession; } }

        //------------------------------------------------------------------------------
        public ConnectionLinks(WoormDocument document) : base()
        {
            this.document = document;
        }

        //------------------------------------------------------------------------------
        public ConnectionLink ExistsConnectionOnAlias(int alias, WoormDocument woorm)
        {
            foreach (ConnectionLink conn in this)
                if (conn.OnAlias == alias && conn.Valid)
                    return conn;
            return null;
        }

        //------------------------------------------------------------------------------
        public ConnectionLink GetConnectionOnAlias(int alias, WoormDocument woorm, int row)
        {
            foreach (ConnectionLink conn in this)
                if (conn.OnAlias == alias && conn.Valid && conn.EvalFilters(woorm, row))
                    return conn;
            return null;
        }

        //------------------------------------------------------------------------------
        public bool Parse(WoormParser lex)
        {
            bool bHaveLinks = lex.Matched(Token.LINKS);
            if (bHaveLinks && !lex.ParseBegin())
                return false;

            while
                (
                    lex.Matched(Token.SELECT) ||    //compatibilita sintassi r5
                    lex.LookAhead(Token.LINKFUNCTION) ||
                    lex.LookAhead(Token.LINKFORM) ||
                    lex.LookAhead(Token.LINKREPORT) ||
                    lex.LookAhead(Token.LINKRADAR) ||
                    lex.LookAhead(Token.LINKURL)
                )
            {
                ConnectionLink connection = new ConnectionLink(this, Document.SymbolTable);
                if (!connection.Parse(lex))
                    return false;

                Add(connection);
            }

            if (bHaveLinks && !lex.ParseEnd())
                return false;

            return true;
        }

        //------------------------------------------------------------------------------
        public bool Unparse(Unparser unparser)
        {
            if (this.Count <= 0)
                return true;

            unparser.WriteTag(Token.LINKS);
            unparser.IncTab();

            unparser.WriteBegin();
            unparser.IncTab();

            foreach (ConnectionLink item in this)
                item.Unparse(unparser);

            unparser.DecTab();
            unparser.WriteEnd();

            unparser.DecTab();
            unparser.WriteLine();

            return true;
        }
    }
}