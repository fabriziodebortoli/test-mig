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

namespace Microarea.DataService.Models
{
    public class Datasource
    {
        public string Namespace;
        ReferenceObjectsPrototype XmlDescription = null;

        public SymbolTable SymTable = new SymbolTable();
          public SymField dsSelectionType     = new SymField("string", "dsSelectionType");
          public SymField dsValue             = new SymField("string", "dsValue");
 
        public TbSession tbSession = null;

        public QueryObject CurrentQuery = null;

        public Datasource(TbSession session)
        {
            SymTable.Add(dsValue); 
            SymTable.Add(dsSelectionType);

            tbSession = session;
        }

        public bool Load(string ns, string selectionType,  string prefix = "")
        {
            Namespace = ns;
            dsSelectionType.Data = selectionType;
            dsValue.Data = prefix;

            XmlDescription = ReferenceObjects.LoadPrototypeFromXml(ns, BasePathFinder.BasePathFinderInstance);
            if (XmlDescription == null)
                return false;

            //Aggiungere alla SymbolTable i parametri espliciti della descrizione
            foreach (IParameter p in XmlDescription.HotLinkParameters)
            {
                SymField paramField = new SymField(p.TbType, p.Name);
                if (p.Optional)
                {
                    //TODO paramField.Data = p.Value;
                }
                SymTable.Add(paramField);
            }

            this.CurrentQuery = new QueryObject(selectionType, SymTable, tbSession, null);

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
