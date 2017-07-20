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

namespace Microarea.Common.Hotlink
{
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

    public class Datasource
    {
        Auxdata auxData;
        public TbSession Session = null;
        public string RadarTable = string.Empty;

        ReferenceObjectsPrototype XmlDescription = null;

        public SymbolTable SymTable = new SymbolTable();
        public SymField selection_type = new SymField("string", "selection_type");
        public SymField like_value = new SymField("string", "like_value");

        public QueryObject CurrentQuery = null;

        public Datasource(TbSession session)
        {
            SymTable.Add(selection_type);
            SymTable.Add(like_value);

            this.Session = session;
        }

        public Datasource(UserInfo ui)
        {
            this.Session = new TbSession(ui, "");
         }
 
        public bool PrepareQuery(IQueryCollection requestQuery, string selectionType = "code")
        {
            string s = requestQuery["selection_type"];
            if (s.IsNullOrEmpty() && selectionType.IsNullOrEmpty())
                return false;

            selection_type.Data = (s.IsNullOrEmpty() ? selectionType : s).ToLower();

            s = requestQuery["like_value"];
            like_value.Data = s != null ? s : "%";

            XmlDescription = ReferenceObjectsList.LoadPrototypeFromXml(Session.Namespace, Session.PathFinder);
            if (XmlDescription == null)
                return false;

            //Vengono aggiunti alla SymbolTable i parametri espliciti della descrizione
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
            }

            //string t = XmlDescription.DbTableName;
            //DBData.DBInfo.GetColumnType(Session.UserInfo.CompanyDbConnection, t, string columnName)

            //viene cercato il corpo della query ------------------
            string selectionName = selection_type.Data as string;
            SelectionMode sm = XmlDescription.GetMode(selectionName);
            if (sm == null)
                return false;
            //-------------------------------

            this.CurrentQuery = new QueryObject(sm.ModeName, SymTable, Session, null);

            if (!this.CurrentQuery.Define(sm.Body))
                return false;

            return true;
        }

        public async Task<bool> PrepareQueryAsync(IQueryCollection requestQuery, string selectionType = "Code", string likeValue = "")
        {
            selection_type.Data = selectionType;
            like_value.Data = likeValue + '%';

            XmlDescription = ReferenceObjectsList.LoadPrototypeFromXml(Session.Namespace, Session.PathFinder);
            if (XmlDescription == null)
                return false;

            if (XmlDescription.IsDatafile)
            {
                return LoadDataFile();
            }

            //Vengono aggiunti alla SymbolTable i parametri espliciti della descrizione
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
            }

            //viene cercato il corpo della query ------------------
            string selectionName = selection_type.Data as string;
            SelectionMode sm = XmlDescription.GetMode(selectionName);
            string query = "";
            if (sm == null)
            {
                if (XmlDescription.SelectionTypeList.Count == 0 && !XmlDescription.ClassType.IsNullOrEmpty())
                {
                  query = await TbSession.GetHotLinkQuery(Session, Session.Namespace, /*aParams*/"", (int)Hotlink.HklAction.DirectAccess);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                this.CurrentQuery = new QueryObject(sm.ModeName, SymTable, Session, null);
                query = sm.Body;
            }


            //------------------------------ 
            if (!this.CurrentQuery.Define(query))
                return false;

            return true;
        }

        //---------------------------------------------------------------------

        public async Task<bool> PrepareRadarAsync(HttpRequest request)
        {
            string query =  request.Form["query"];
            string columnInfos = request.Form["columnInfos"];

            this.CurrentQuery = new QueryObject("radar", SymTable, Session, null);
            //------------------------------ 
            if (!this.CurrentQuery.Define(query))
                return false;

            return true;
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
                return false;

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

            ArrayList columns = new ArrayList();
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
        public bool GetCompactJson(out string records)
        {
            records = string.Empty;

            if (XmlDescription.IsDatafile)
            {
                return GetDataFileJson(out records);
            }

            if (!CurrentQuery.Open())
                return false;

            ArrayList columns = new ArrayList();
            CurrentQuery.EnumColumns(columns);

            //emit json record header (localized title column, column name, datatype column
            records = "{" +
                   XmlDescription.DbFieldName.Replace('.', '_').ToJson("key");

            records += ", \"columns\":[";
            bool first = true;
            foreach (SymField f in columns)
            {
                if (first)
                    first = false;
                else
                    records += ',';

                string fname = f.Name.Replace('.', '_').ToJson("id");

                records += '{' +
                           fname +
                           ',' +
                           f.Title.ToJson("caption", false, true) +   ","+
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
