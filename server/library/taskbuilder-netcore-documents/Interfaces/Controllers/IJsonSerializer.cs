using TaskBuilderNetCore.Documents.Interfaces;

namespace TaskBuilderNetCore.Documents.Interfaces
{
    //====================================================================================    
    public interface IJsonSerializer
    {
        void Deserialize(IDocument document);
        void Serialize(IDocument document);
    }
}