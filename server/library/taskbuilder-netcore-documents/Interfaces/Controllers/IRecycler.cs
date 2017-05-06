using TaskBuilderNetCore.Documents.Interfaces;

namespace TaskBuilderNetCore.Documents.Interfaces
{
    public interface IRecycler
    {
        bool IsAvailable(IDocument document);
        bool IsRemovable(IDocument document);
        bool IsRecyclable(IDocument document);
        void Recycle(IDocument document);
    }
}