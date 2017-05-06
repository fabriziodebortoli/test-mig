using TaskBuilderNetCore.Documents.Interfaces;

namespace TaskBuilderNetCore.Documents.Interfaces
{
    public interface IJsonSerializer
    {
        void Deserialize(IDocument bo);
        void Serialize(IDocument bo);
    }
}