﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TaskBuilderNetCore.Interfaces;

using Microarea.Common.Applications;
using Microarea.Common.CoreTypes;
using Microarea.Common.Hotlink;
using Microarea.Common.Lexan;
using Microarea.Common.ExpressionManager;
using Microarea.Common.NameSolver;
using Microsoft.AspNetCore.Http;
using System.Collections;

namespace Microarea.DataService.Models
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

        public bool PrepareQuery(IQueryCollection requestQuery)
        {
            string s = requestQuery["selection_type"];
            selection_type.Data =  s;
            s = requestQuery["like_value"];
            like_value.Data = s;

            XmlDescription = ReferenceObjects.LoadPrototypeFromXml(Session.Namespace, Session.PathFinder);
            if (XmlDescription == null)
                return false;

             /*    nell'esempio       
*              disabled = HttpContext.Request.Query["disabled"];
            good_type = HttpContext.Request.Query["good_type"]; 
            */
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
            SelectionMode  sm = XmlDescription.GetMode(selectionName); 
            if (sm == null)
                return false;
            //-------------------------------

            this.CurrentQuery = new QueryObject(sm.ModeName, SymTable, Session, null);

            if (!this.CurrentQuery.Define(sm.Body))
                return false;

            return true;
        }

        public bool EnumSelectionTypes(out string selections)
        {
            selections = string.Empty;

            XmlDescription = ReferenceObjects.LoadPrototypeFromXml(Session.Namespace, Session.PathFinder);
            if (XmlDescription == null)
                return false;

            if (XmlDescription.SelectionTypeList.Count == 0)
                return false;

            bool first = true;
            selections = "{\"selections\":[";
            foreach (SelectionType st in XmlDescription.SelectionTypeList)
            {
                if (!first) selections += ',';
                selections += '\"' + st.SelectionName + '\"';
                first = false;
            }
            selections += "]}";

            return true;
        }

        //---------------------------------------------------------------------
        public bool GetCompactJson(out string records)
        {
            records = string.Empty;

            if (!CurrentQuery.Open())
                return false;

            ArrayList columns = new ArrayList();
            CurrentQuery.EnumColumns(columns);

            //emit json record header (localized title column, column name, datatype column
            bool first = true;
            records = "{\"titles\":[";
            foreach (SymField f in columns)
            {
                if (!first) records += ',';
                records += '\"' + f.Name + '\"';
                first = false;
            }
            records += "],\n\"rows\":[";

            first = true;
            while (CurrentQuery.Read())
            {
                //emit json record
                foreach (SymField f in columns)
                {
                    object o = f.Data;

                    if (first)
                        records += '[';
                    else
                        records += ',';

                    records += o.ToString();
                }
                records += "]\n";
            }
            records += "]}";

            CurrentQuery.Close();
            return true;
        }

        //---------------------------------------------------------------------
        public bool GetKendoJson(out string records)
        {
            //TODO occorre cambiare la sintassi
            return GetCompactJson(out records);
        }
    }
}
