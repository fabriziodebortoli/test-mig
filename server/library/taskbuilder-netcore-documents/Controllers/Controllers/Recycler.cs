using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskBuilderNetCore.Documents.Model;
using TaskBuilderNetCore.Documents.Interfaces;

namespace TaskBuilderNetCore.Documents.Controllers
{
    [Name("Recycler"), Description("It manages document pool.")]
    public class Recycler : Controller, IRecycler
    {
        public bool IsAvailable(IDocument bo)
        {
            // logiche di onidle o utilizzo a scadenza o a references
            return true;
        }

        public bool IsRemovable(IDocument bo)
        {
            // logiche di onidle o utilizzo a scadenza o a references
            return true;
        }
    }
}
