using TaskBuilderNetCore.Documents.Interfaces;

namespace TaskBuilderNetCore.Documents.Interfaces
{
    public interface IRecycler
    {
        bool IsAvailable(IDocument bo);
        bool IsRemovable(IDocument bo);
    }
}