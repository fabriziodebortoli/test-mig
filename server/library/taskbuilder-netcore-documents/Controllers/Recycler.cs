using TaskBuilderNetCore.Documents.Model.Interfaces;
using TaskBuilderNetCore.Documents.Controllers.Interfaces;
using TaskBuilderNetCore.Documents.Model;

namespace TaskBuilderNetCore.Documents.Controllers
{
    [Name("Recycler"), Description("It manages document recycling strategies.")]
    //====================================================================================    
    public class Recycler : Controller, IRecycler
    {
        //-----------------------------------------------------------------------------------------------------
        public bool IsAssignable(IComponent component, ICallerContext context)
        {
            if (!IsAvailable(component))
                return false;

            return context.Identity.CompareTo(component.CallerContext.Identity) == 0;
        }

    

        //-----------------------------------------------------------------------------------------------------
        public bool IsAvailable(IComponent component)
        {
            // logiche di onidle o utilizzo a scadenza o a references
            return true;
        }


        //-----------------------------------------------------------------------------------------------------
        public bool IsRemovable(IComponent component)
        {
            // logiche di onidle o utilizzo a scadenza o a references
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        public bool IsRecyclable(IComponent component)
        {
            // logiche di onidle o utilizzo a scadenza o a references
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        public void Recycle(IComponent component)
        {
            // se e' dirty salvataggio dello stato e liberazione
            component.Clear();
        }
    }
}
