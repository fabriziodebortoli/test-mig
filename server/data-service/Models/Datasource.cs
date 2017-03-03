using System;
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
            selection_type.Data = requestQuery["selection_type"];
            like_value.Data = requestQuery["like_value"];

            XmlDescription = ReferenceObjects.LoadPrototypeFromXml(Session.Namespace, Session.PathFinder);
            if (XmlDescription == null)
                return false;

             /*    nell'esempio       
*              disabled = HttpContext.Request.Query["disabled"];
            good_type = HttpContext.Request.Query["good_type"]; 
            */
            //Vengono aggiunti alla SymbolTable i parametri espliciti della descrizione
           foreach (IParameter p in XmlDescription.HotLinkParameters)
            {
                SymField paramField = new SymField(p.TbType, p.Name);
                if (p.Optional)
                {
                    //TODO parsed optional value paramField.Data = p.Value;
                }

                paramField.Data = requestQuery[p.Name];

                SymTable.Add(paramField);
            }

            //viene cercato il corpo della query ------------------
            string modeName = string.Empty;
            string sQuery = string.Empty;

            string selectionName = selection_type.Data as string;
            for (int i = 0; i < XmlDescription.SelectionTypeList.Count; i++)
            {
                SelectionType st = XmlDescription.SelectionTypeList[i];
                if (string.Compare(selectionName, st.SelectionName) == 0)
                {
                    modeName = st.ModeName;
                    break;
                }
            }
            if (modeName == string.Empty)
                return false;

            for (int i = 0; i < XmlDescription.SelectionModeList.Count; i++)
            {
                SelectionMode sm = XmlDescription.SelectionModeList[i];
                if (string.Compare(modeName, sm.ModeName) == 0)
                {
                    sQuery = sm.Body;
                    break;
                }
            }
            if (sQuery == string.Empty)
                return false;
            //-------------------------------

            this.CurrentQuery = new QueryObject(modeName, SymTable, Session, null);

            if (!this.CurrentQuery.Define(sQuery))
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
