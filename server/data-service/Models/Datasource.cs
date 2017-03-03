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

namespace Microarea.DataService.Models
{
    public class Datasource
    {
        public TbSession Session = null;
       
        ReferenceObjectsPrototype XmlDescription = null;

        public SymbolTable SymTable = new SymbolTable();
          public SymField selection_type     = new SymField("string", "selection_type");
          public SymField like_value         = new SymField("string", "like_value");
 
        public QueryObject CurrentQuery = null;

        public Datasource(TbSession session)
        {
            SymTable.Add(selection_type);
            SymTable.Add(like_value); 
 
            this.Session = session;
        }

        public bool Load(IQueryCollection requestQuery)
        {
            selection_type.Data = requestQuery["selection_type"];
            like_value.Data     = requestQuery["like_value"];

            XmlDescription = ReferenceObjects.LoadPrototypeFromXml(Session.Namespace, Session.PathFinder);
            if (XmlDescription == null)
                return false;

            //Aggiungere alla SymbolTable i parametri espliciti della descrizione
            /*           
*              disabled = HttpContext.Request.Query["disabled"];
            good_type = HttpContext.Request.Query["good_type"]; 
            */
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

            this.CurrentQuery = new QueryObject(selection_type.Data as string, SymTable, Session, null);

            string sQuery = string.Empty;
            //TODO manca la lettura ed il metodo
            //sQuery = XmlDescription.GetModeQueryText(selectionType);
            if (sQuery == string.Empty)
                return false;

            if (!this.CurrentQuery.Define(sQuery))
                return false;

            return true;
        }
    }
}
