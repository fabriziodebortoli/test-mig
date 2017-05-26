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

namespace Microarea.Common.Hotlink
{
    public class Datasource
    {
        public TbSession Session = null;

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

        public bool PrepareQuery(IQueryCollection requestQuery, string selectionType = "Code")
        {
            string s = requestQuery["selection_type"];
            if (s.IsNullOrEmpty() && selectionType.IsNullOrEmpty())
                return false;

            selection_type.Data = (s.IsNullOrEmpty() ? selectionType : s);

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
            SelectionMode  sm = XmlDescription.GetMode(selectionName); 
            if (sm == null)
                return false;
            //-------------------------------

            this.CurrentQuery = new QueryObject(sm.ModeName, SymTable, Session, null);

            if (!this.CurrentQuery.Define(sm.Body))
                return false;

            return true;
        }

        public bool PrepareQuery(/*IQueryCollection requestQuery,*/ string selectionType = "Code", string likeValue = "")
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

                string sp = null; // requestQuery[p.Name];

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
            if (sm == null)
                return false;
            //-------------------------------

            this.CurrentQuery = new QueryObject(sm.ModeName, SymTable, Session, null);

            if (!this.CurrentQuery.Define(sm.Body))
                return false;

            return true;
        }

        //---------------------------------------------------------------------
        public bool LoadDataFile()
        {
           NameSpace ns = new NameSpace(XmlDescription.Datafile, NameSpaceObjectType.DataFile);
           if (!ns.IsValid())
               return false;

           string path = this.Session.PathFinder.GetStandardDataFilePath(ns.Application, ns.Module, this.Session.UserInfo.UserUICulture.ToString()) + 
                Path.DirectorySeparatorChar +
                ns.ObjectName + ".xml";

            //carica l'xml del datafile in una struttura (data member di questa classe)
            // throw new NotImplementedException();
            return false;
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
                             '}' ;

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
                             f.Name.Replace('.', '_').ToJson("id") +
                             ',' +
                             f.Title.ToJson("caption", false, true) +
                         '}';
            }
            list += "]}";

            CurrentQuery.Close();
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
                           f.Title.ToJson("caption", false, true) +
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

                    if (first)
                    {
                        rows += '{';
                        first = false;
                    }
                    else
                    {
                        rows += ',';
                    }

                    rows += '\"' + f.Name.Replace('.', '_') + "\":";

                    if (
                            string.Compare(f.DataType, "date", true) == 0 ||
                            string.Compare(f.DataType, "datetime", true) == 0
                       )
                    {
                        DateTime dat = (DateTime)o;
                        string s = dat.ToString("yyyy-MM-dd");

                        rows += s.ToJson(null, false, true);
                    }
                    else if (string.Compare(f.DataType, "string", true) == 0)
                    {
                        string s = o.ToString();

                        //if (string.Compare(f.WoormType, "double", true)

                        rows += s.ToJson(null, false, true);
                    }
                   else if (string.Compare(f.DataType, "double", true) == 0)
                    {
                        double d = (double)o;
                        

                        rows += d.ToJson();
                    }
                    else
                        rows += o.ToString();
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
            //dalla struttura letta prima a json stessa sintassi dell'altro metodo
            throw new NotImplementedException();
        }

        //---------------------------------------------------------------------
        public bool GetKendoJson(out string records)
        {
            //TODO occorre cambiare la sintassi
            return GetCompactJson(out records);
        }
    }
}
