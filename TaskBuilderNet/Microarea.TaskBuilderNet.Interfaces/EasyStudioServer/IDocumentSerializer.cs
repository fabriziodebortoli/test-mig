using Microarea.TaskBuilderNet.Interfaces.Model;

namespace Microarea.TaskBuilderNet.Interfaces.EasyStudioServer
{
    public interface IDocumentSerializer : ISerializer
    {
        bool DeclareDocument(IDocumentInfo docInfo);
        bool Create(IDocument doc);
        bool Create(IDocument doc, string fileName, string defaultCode = null);
    }
}