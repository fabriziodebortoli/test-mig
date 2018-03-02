using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskBuilderNetCore.Documents.Model.TbModel
{
    //====================================================================================    
    // puo' essere un component apportatore di data model e di altri componenti
    public class ClientDoc : DocumentComponent
    {
        //-----------------------------------------------------------------------------------------------------
        public ClientDoc()
        {

        }

        //-----------------------------------------------------------------------------------------------------
        protected override bool OnInitialize()
        {
            bool initialized = base.OnInitialize();
            Document.DataModelAttached += Document_DataModelAttached;
            
            return initialized;
        }

   
        //-----------------------------------------------------------------------------------------------------
        private void Document_DataModelAttached(object sender, EventArgs e)
        {
            OnAttachData();
        }

        //-----------------------------------------------------------------------------------------------------
        protected virtual bool OnAttachData()
        {
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (!Disposed && disposing)
            {
                Document.DataModelAttached -= Document_DataModelAttached;
            }
            base.Dispose(disposing);
        }
    }
}
