using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskBuilderNetCore.Documents.Model;
using TaskBuilderNetCore.Documents.Interfaces;

namespace TaskBuilderNetCore.Documents.Controllers
{
    [Name("Recycler"), Description("It manages document recycling strategies.")]
    //====================================================================================    
    public class Recycler : Controller, IRecycler
    {
        //-----------------------------------------------------------------------------------------------------
        public bool IsAssignable(IDocument document, ICallerContext context)
        {
            if (!IsAvailable(document))
                return false;

            return context.IsSameIdentity(document.CallerContext);
        }

        //-----------------------------------------------------------------------------------------------------
        public bool IsAvailable(IDocument document)
        {
            // logiche di onidle o utilizzo a scadenza o a references
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        public bool IsRemovable(IDocument document)
        {
            // logiche di onidle o utilizzo a scadenza o a references
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        public bool IsRecyclable(IDocument document)
        {
            // logiche di onidle o utilizzo a scadenza o a references
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        public void Recycle(IDocument document)
        {
            // se e' dirty salvataggio dello stato e liberazione
            document.Clear();
        }
    }
}
