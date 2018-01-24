
using TaskBuilderNetCore.Interfaces;
using TaskBuilderNetCore.Documents.Model.Interfaces;
namespace TaskBuilderNetCore.EasyStudio.Interfaces
{
    public interface IDocumentSerializer : ISerializer
    {
        bool DeclareDocument(IDocumentInfo docInfo);
        bool Create(IDocument doc);
        bool Create(IDocument doc, string fileName, string defaultCode = null);
    }
}