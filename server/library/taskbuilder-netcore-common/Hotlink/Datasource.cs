using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;

using Microsoft.AspNetCore.Http;

using TaskBuilderNetCore.Interfaces;

using Microarea.Common.Generic;
using Microarea.Common.Applications;
using Microarea.Common.CoreTypes;
using System.IO;
using System.Xml;
using Microarea.Common.StringLoader;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microarea.Common.Hotlink
{
    //-------------------------------------------------------------------------

    public class SelectionFieldType
    {
        public string Name;
        public string Key;
        public string Hidden;
        public string Type;

        public SelectionFieldType(string name, string key, string hidden, string type)
        {
            Name = name;
            Key = key;
            Hidden = hidden;
            Type = type;
        }
    }

    public class FieldTypeList
    {
        public List<SelectionFieldType> FieldTypes = new List<SelectionFieldType>();
    }

    public class SelectionField
    {
        public string NameField;
        public string ValueField;

        public SelectionField(string name, string value)
        {
            NameField = name;
            ValueField = value;
        }
    }

    public class ElementList
    {
        public List<SelectionField> Fields = new List<SelectionField>();

    }

    public class Auxdata
    {
        public List<SelectionFieldType> Headers = new List<SelectionFieldType>();
        public List<ElementList> Elements = new List<ElementList>();
    }

    //-------------------------------------------------------------------------
    public class radarInfo
    {
       public List<ColumnType> columnInfos { get; set; }
       public string query { get; set; }
       public List<string> recordKeys { get; set; }
    }

    public class ResponseRadarInfo
    {
        public radarInfo radarInfo { get; set; }
    }

    //-------------------------------------------------------------------------

    public class FilterField
    {
        public string field { get; set; }

        [JsonProperty(PropertyName = "operator")]
        public string Operator { get; set; }
 
       [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }
   }

    public class CustomFilters
    {
        public List<FilterField> filters { get; set; }

        public string logic { get; set; }
    }


    public class GridCustomFilters
    {
        public CustomFilters cf { get; set; }
    }

    //-------------------------------------------------------------------------

    public class Datasource
    {
        Auxdata auxData;
        public TbSession Session = null;
        public string RadarTable = string.Empty;

        ReferenceObjectsPrototype XmlDescription = null;

        public SymbolTable SymTable = new SymbolTable();
        public SymField selection_type = new SymField("string", "selection_type");
        public SymField filter_value = new SymField("string", "filter_value");

        public QueryObject CurrentQuery = null;

        public Datasource(TbSession session)
        {
            SymTable.Add(selection_type);
            SymTable.Add(filter_value);

            this.Session = session;
        }

        public Datasource(UserInfo ui)
        {
            this.Session = new TbSession(ui, "");
         }
 
        public async Task<bool> PrepareQueryAsync(IQueryCollection requestQuery, string selectionType = "code")
        {
            XmlDescription = ReferenceObjectsList.LoadPrototypeFromXml(Session.Namespace, Session.PathFinder);
            if (XmlDescription == null)
            {
                Debug.Fail("Missing Xml Description of " + Session.Namespace);
                return false;
            }

            selection_type.Data = selectionType;

            CustomFilters customFilters = null;
            string cf = requestQuery["customFilters"];
            if (cf.IsNullOrEmpty() || cf == "{}" || /*tapullo*/ cf == "\"\"")
                cf = string.Empty;
            else
            {
                customFilters = JsonConvert.DeserializeObject<CustomFilters>(cf);
                if (customFilters == null || customFilters.filters == null)
                    Debug.Fail("Missing Xml Description of " + Session.Namespace);
            }

            string likeValue = requestQuery["filter"];
            if (String.IsNullOrEmpty(likeValue) || /*tapullo*/ likeValue== "\"\"") 
                likeValue = string.Empty;

            if (!selectionType.CompareNoCase("direct"))
                filter_value.Data = likeValue + "%";
            else
                filter_value.Data = likeValue;

            //Paging
            bool isPaged = false;
            int pageNum = 0, rowsPerPage = 0;
            if (!string.IsNullOrEmpty(requestQuery["page"]))
            {
                pageNum = 1;
                int.TryParse(requestQuery["page"], out pageNum);
                isPaged = true;

                rowsPerPage = 10;
                if (!string.IsNullOrEmpty(requestQuery["per_page"]))
                {
                    int.TryParse(requestQuery["per_page"], out rowsPerPage);
                }
            }          
 
            if (XmlDescription.IsDatafile)
            {
                // TODO RSWEB: paginazione anche nella lettura dati da xml ?
                return LoadDataFile();
            }

            //Vengono aggiunti alla SymbolTable i parametri espliciti della descrizione
            FunctionPrototype args = new FunctionPrototype();
            foreach (IParameter p in XmlDescription.Parameters)
            {
                SymField paramField = new SymField(p.TbType, p.Name);

                string sp = requestQuery[p.Name];

                if (sp == null)
                {
                    if (p.Optional)
                    {
                        paramField.Assign(p.ValueString);
                    }
                }
                else
                {
                    paramField.Assign(sp);
                }

                SymTable.Add(paramField);

                Parameter param = new Parameter(p.Name, p.TbType);
                param.ValueString = SoapTypes.To(paramField.Data);

                args.Parameters.Add(param);
            }

            //viene cercato il corpo della query ------------------
            SelectionMode sm = XmlDescription.GetMode(selectionType);
            string query = string.Empty;
            if (sm == null)
            {
                if (!XmlDescription.ClassType.IsNullOrEmpty())
                {
                    string documentId = requestQuery["documentID"].ToString();
                    //if (documentId.IsNullOrEmpty())
                    //{
                    //    return false;
                    //}

                    string hklName = requestQuery["hklName"].ToString();
                    //if (hklName.IsNullOrEmpty())
                    //{
                    //    return false;
                    //}

                    if (hklName.IsNullOrEmpty() != documentId.IsNullOrEmpty())
                    {
                        Debug.Fail("Hotlink of Document " + hklName);
                        return false;
                    }

                    Hotlink.HklAction hklAction = Hotlink.HklAction.Code;
                    if (selectionType.CompareNoCase("description"))
                        hklAction = Hotlink.HklAction.Description;
                    else if (selectionType.CompareNoCase("combo"))
                        hklAction = Hotlink.HklAction.Combo;
                    else if (selectionType.CompareNoCase("direct"))
                        hklAction = Hotlink.HklAction.DirectAccess;

                    query = await TbSession.GetHotLinkQuery(Session, args.Parameters.Unparse(), (int)hklAction, likeValue, documentId, hklName);
                    if (query.IsNullOrEmpty())
                    {
                        Debug.Fail("GetHotLinkQuery failed ");
                        return false;
                    }

                    JObject jObject = JObject.Parse(query);
                    if (jObject == null)
                    {
                        Debug.Fail("It fails to parse HotLink Query");
                        return false;
                    }

                    query = jObject.GetValue("query")?.ToString();
                    if (query.IsNullOrEmpty())
                    {
                        Debug.Fail("HotLink Query is empty");
                        return false;
                    }

                    this.CurrentQuery = new QueryObject("tb", SymTable, Session, null);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                query = sm.Body;
                this.CurrentQuery = new QueryObject(sm.ModeName, SymTable, Session, null);
            }

            //------------------------------ 
            string customWhere = string.Empty;
            if (customFilters != null && customFilters.filters != null && customFilters.filters.Count > 0)
            {
                bool first = true;
                foreach (FilterField ff in customFilters.filters)
                {
                    if (!first)
                    {
                        customWhere += ' ' + customFilters.logic + ' ';
                    }
                    else
                        first = false;

                    if (ff.Operator.CompareNoCase("Contains"))
                        customWhere += ff.field + string.Format(" LIKE '%{0}%'", ff.Value);
                    else if (ff.Operator.CompareNoCase("DoesNotContain"))
                        customWhere += ff.field + string.Format("NOT LIKE '%{0}%'", ff.Value);
                    else if (ff.Operator.CompareNoCase("IsEqualTo"))
                        customWhere += ff.field + string.Format(" = {0}", ff.Value);
                    else if (ff.Operator.CompareNoCase("IsNotEqualTo"))
                        customWhere += ff.field + string.Format(" <> {0}", ff.Value);
                    else if (ff.Operator.CompareNoCase("StartsWith"))
                        customWhere += ff.field + string.Format(" LIKE '{0}%'", ff.Value);
                    else if (ff.Operator.CompareNoCase("EndsWith"))
                        customWhere += ff.field + string.Format(" LIKE '%{0}'", ff.Value);
                    else if (ff.Operator.CompareNoCase("IsNull"))
                        customWhere += ff.field + string.Format(" IS NULL", ff.Value);
                    else if (ff.Operator.CompareNoCase("IsNotNull"))
                        customWhere += ff.field + string.Format(" IS NOT NULL", ff.Value);
                    else if (ff.Operator.CompareNoCase("IsEmpty"))
                        customWhere += ff.field + string.Format(" = ''", ff.Value);
                    else if (ff.Operator.CompareNoCase("IsNotEmpty"))
                        customWhere += ff.field + string.Format(" <> ''", ff.Value);
                }
            }
            
            if (!this.CurrentQuery.Define(query))
            {
                Debug.Fail("DS fails to prepare hotlink query");
                return false;
            }

            this.CurrentQuery.SetCustomWhere(customWhere);
            if (isPaged)
            {
                this.CurrentQuery.SetPaging(pageNum, rowsPerPage);
            }

            return true;
        }

        //---------------------------------------------------------------------
        public async Task<ResponseRadarInfo> PrepareRadar(IQueryCollection requestQuery, string nsDoc = "", string name = "")
        {
            string documentId = requestQuery["documentID"].ToString();
            if (documentId.IsNullOrEmpty())
            {
                Debug.Fail("Radar called without DocumentID");
                return null;
            }

            //Paging
            bool isPaged = false;
            int pageNum = 0, rowsPerPage = 0;
            if (!string.IsNullOrEmpty(requestQuery["page"]))
            {
                pageNum = 1;
                int.TryParse(requestQuery["page"], out pageNum);
                isPaged = true;

                rowsPerPage = 10;
                if (!string.IsNullOrEmpty(requestQuery["per_page"]))
                {
                    int.TryParse(requestQuery["per_page"], out rowsPerPage);
                }
            }

            string response = await TbSession.GetRadarQuery(Session, documentId, name);
            if (response.IsNullOrEmpty())
            {
                Debug.Fail("GetRadarQuery failed");
                return null;
            }

            ResponseRadarInfo responseRadarInfo = JsonConvert.DeserializeObject<ResponseRadarInfo>(response);
            if (responseRadarInfo == null || responseRadarInfo.radarInfo == null || responseRadarInfo.radarInfo.query.IsNullOrEmpty())
            {
                Debug.Fail("It fails to parse RadarInfo");
                return null;
            }

            this.CurrentQuery = new QueryObject("radar", SymTable, Session, null);

            if (!this.CurrentQuery.Define(responseRadarInfo.radarInfo.query, responseRadarInfo.radarInfo.columnInfos))
            {
                Debug.Fail("DS fails to prepare radar query");
                return null;
            }

            if (isPaged)
            {
                this.CurrentQuery.SetPaging(pageNum, rowsPerPage);
            }

            return responseRadarInfo;
        }

        //--------------------------------------------------------------------
        static private string TableName(string name)
        {
            int pos = name.IndexOf('.');
            return pos >= 0 ? name.Substring(0, pos) : "";
        }

        public bool LoadDataFile()
        {
            NameSpace ns = new NameSpace(XmlDescription.Datafile, NameSpaceObjectType.DataFile);
            if (!ns.IsValid())
                return false;

            IBaseModuleInfo mi = Session.PathFinder.GetModuleInfo(ns);
            if (mi == null)
                return false;

            string path = this.Session.PathFinder.GetStandardDataFilePath(ns.Application, ns.Module, this.Session.UserInfo.UserUICulture.ToString()) +
                 Path.DirectorySeparatorChar +
                 ns.ObjectName + ".xml";
            if (!File.Exists(path))
                return false;

            // restituisce il dom già tradotto per i Tag o gli Attribute che sono localizzati
            LocalizableXmlDocument dom = new LocalizableXmlDocument(ns.Application, ns.Module, Session.PathFinder);
            dom.Load(path);

            // cerca con XPath solo le funzioni con un dato nome per poi selezionare quella con i parametri giusti
            XmlNode root = dom.DocumentElement;
            string xpath = string.Format
                (
                "/{0}/{1}",
                ReferenceObjectsXML.Element.Auxdata,
                ReferenceObjectsXML.Element.Header
                );

            //----------------------------------
            auxData = new Auxdata();

            // se non esiste la sezione allora il ReferenceObject è Undefined
            XmlNodeList headers = root.SelectNodes(xpath);
            if (headers == null) return false;

            foreach (XmlElement head in headers)
            {

                foreach (XmlElement sel in head.ChildNodes)
                {
                    string selName = sel.GetAttribute("name");
                    string selKey = sel.GetAttribute("key");
                    string selHidden = sel.GetAttribute("hidden");
                    string selType = sel.GetAttribute("type");

                    SelectionFieldType fieldType = new SelectionFieldType(selName, selKey, selHidden, selType);
                    auxData.Headers.Add(fieldType);

                }
            }
            //----------------------------------------
            string xpath2 = string.Format
                (
                 "/{0}/{1}/{2}",
                 ReferenceObjectsXML.Element.Auxdata,
                 ReferenceObjectsXML.Element.Elements,
                 ReferenceObjectsXML.Element.Elem
                );

            // se non esiste la sezione allora il ReferenceObject è Undefined
            XmlNodeList elements = root.SelectNodes(xpath2);
            if (elements == null) return false;

            foreach (XmlElement elem in elements)
            {
                ElementList fieldList = new ElementList();
                auxData.Elements.Add(fieldList);

                foreach (XmlElement sel in elem.ChildNodes)
                {
                    string selName = sel.GetAttribute("name");
                    string selValue = sel.InnerText;

                    SelectionField field = new SelectionField(selName, selValue);
                    fieldList.Fields.Add(field);
                }
             }
            return true;
        }

        //---------------------------------------------------------------------
        public bool GetSelectionTypes(out string list)
        {
            list = string.Empty;

            XmlDescription = ReferenceObjectsList.LoadPrototypeFromXml(Session.Namespace, Session.PathFinder);
            if (XmlDescription == null)
                return false;

            if (XmlDescription.SelectionTypeList.Count == 0)
            {
                list = "{\"selections\":[";
               
                list += '\"' + "code" + '\"' + ',';
                list += '\"' + "description" + '\"' + ',';
                list += '\"' + "combo" + '\"';

                list += "]}";

                return true;
            }

            bool first = true;
            list = "{\"selections\":[";
            foreach (SelectionType st in XmlDescription.SelectionTypeList)
            {
                if (!first) list += ',';
                list += '\"' + st.SelectionName + '\"';
                first = false;
            }
            list += "]}";

            return true;
        }

        //---------------------------------------------------------------------
        public bool GetParameters(out string list)
        {
            list = string.Empty;

            XmlDescription = ReferenceObjectsList.LoadPrototypeFromXml(Session.Namespace, Session.PathFinder);
            if (XmlDescription == null)
                return false;

            if (XmlDescription.SelectionTypeList.Count == 0)
                return false;

            bool first = true;
            list = "{\"parameters\":[";
            foreach (Parameter par in XmlDescription.Parameters)
            {
                if (!first) list += ',';

                string sp = '{' + par.Name.ToJson("id") + ',' +
                                  par.Title.ToJson("title", false, true) + ',' +
                                  par.Type.ToJson("type") +
                             '}';

                list += sp;

                first = false;
            }
            list += "]}";

            return true;
        }

        //---------------------------------------------------------------------
        public bool GetColumns(out string list)
        {
            list = string.Empty;

            if (!CurrentQuery.Open())
                return false;

            List<SymField> columns = new List<SymField>();
            CurrentQuery.EnumColumns(columns);
            CurrentQuery.Close();
 
            //emit json record header (localized title column, column name, datatype column
            list = "{\"columns\":[";
            bool first = true;
            foreach (SymField f in columns)
            {
                if (first)
                    first = false;
                else
                    list += ',';

                list += '{' +
                             f.Name.Replace('.', '_').ToJson("id") +  ',' +
                             f.Title.ToJson("caption", false, true) + ',' +
                             f.DataType.ToJson("type", false, true) +
                         '}';
            }
            list += "]}";

            return true;
        }

        //---------------------------------------------------------------------
        public bool GetRowsJson(out string records, List<string> recordKeys = null)
        {
            records = string.Empty;

            if (XmlDescription != null && XmlDescription.IsDatafile)
            {
                return GetDataFileJson(out records);
            }

            if (!CurrentQuery.Open())
                return false;

            List<SymField> columns = new List<SymField>();
            CurrentQuery.EnumColumns(columns);

            //emit json record header (localized title column, column name, datatype column
            records = "{";

            if (XmlDescription != null && columns.Count > 0)
            {
                SymField f0 = columns[0] as SymField;
                int idx = f0.Name.IndexOf('.');
                if (idx > 0)
                    records += XmlDescription.DbFieldName.Replace('.', '_').ToJson("key") + ',';
                else
                    records += XmlDescription.DbFieldName.Mid(idx + 1).ToJson("key") + ',';
            }
            if (recordKeys != null)
            {
                //TODO RSWEB - aggiungere segmenti chiave primaria - unificare sintassi con la precedente
            }

            records += "\"columns\":[";
            bool first = true;
            foreach (SymField f in columns)
            {
                if (first)
                    first = false;
                else
                    records += ',';

                string fname = f.Name.Replace('.', '_').ToJson("id");
                string title = f.Title;
                if (title.IsNullOrWhiteSpace())
                {
                    int pos = f.Name.LastIndexOf('.');
                    if (pos > -1)
                        title = f.Name.Mid(pos + 1);
                    else
                        title = f.Name;
                }

                records += '{' +
                           fname +
                           ',' +
                           title.ToJson("caption", false, true) +   ","+
                           f.WoormType.ToJson("type", false, false) +
                           '}';
            }
            records += "],\n\"rows\":[";

            string rows = string.Empty;
            while (CurrentQuery.Read())
            {
                //emit json record
                first = true;
                foreach (SymField f in columns)
                {
                    object o = f.Data;
                    if (o == null)
                    {
                        Debug.Fail("Empty value for " + f.Name);
                        continue;
                    }

                    if (first)
                    {
                        rows += '{';
                        first = false;
                    }
                    else
                    {
                        rows += ',';
                    }
                 
                    rows += o.ToJson(f.Name.Replace('.', '_'));
                }
                rows += "},\n";
            }
            if (rows != string.Empty)
                rows = rows.Remove(rows.Length - 2); //ultima ,

            records += rows + "]}";

            CurrentQuery.Close();
            return true;
        }

        private bool GetDataFileJson(out string records)
        {
            records = "{" +
                    XmlDescription.DbFieldName.Replace('.', '_').ToJson("key");

            records += ", \"columns\":[";
            bool first = true;
            foreach (SelectionFieldType ft in auxData.Headers)
            {
                if (ft.Hidden == "1") continue;

                if (first)
                    first = false;
                else
                    records += ',';

                records += '{' +
                           ft.Name.ToJson("id") +
                           ',' +
                           ft.Name.ToJson("caption") +
                           '}';
            }
            records += "],\n\"rows\":[";

            string rows = string.Empty;
            foreach (ElementList el in auxData.Elements)
            {
                bool first2 = true;
                foreach (SelectionField f in el.Fields)
                {
                    if (first2)
                    {
                        rows += '{';
                        first2 = false;
                    }
                    else
                        rows += ',';

                    rows += f.ValueField.ToJson(f.NameField, false, true);
                }
                first2 = true;
                rows += "},\n";
            }

            if (rows != string.Empty)
                rows = rows.Remove(rows.Length - 2); //ultima ,

            records += rows + "]}";

            return true;
        }

        //---------------------------------------------------------------------
     }
}
