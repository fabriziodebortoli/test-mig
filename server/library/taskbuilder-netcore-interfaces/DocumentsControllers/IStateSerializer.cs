using System.Threading.Tasks;
using TaskBuilderNetCore.Documents.Model.Interfaces;

namespace TaskBuilderNetCore.Documents.Controllers.Interfaces
{
    //====================================================================================    
    public interface IStateSerializer
    {
        void Add(IDocument document);
        void Remove(IDocument document);

        string GetJson(IComponent component);
        void LoadJson(IComponent component, string json);
        void LoadState(IComponent component);
        Task<bool> SaveState(IComponent component);
    }
}   