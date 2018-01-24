using TaskBuilderNetCore.Documents.Model.Interfaces;

namespace TaskBuilderNetCore.Documents.Controllers.Interfaces
{
    //====================================================================================    
    public interface IRecycler
    {
        bool IsAssignable(IComponent component, ICallerContext context);
        bool IsAvailable(IComponent component);
        bool IsRemovable(IComponent component);
        bool IsRecyclable(IComponent component);
        void Recycle(IComponent component);
    }
}