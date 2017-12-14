using Microarea.TaskBuilderNet.Core.EasyStudioServer.Services;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Interfaces.Model;
using System.IO;

namespace Microarea.TaskBuilderNet.Core.EasyStudioServer.Serializers
{
    //====================================================================
    public class DocumentSerializer : BaseDocumentSerializer
    {
        const string templateFile = "Document.cs";
      
        //---------------------------------------------------------------
        public DocumentSerializer()
        {
        }

        //---------------------------------------------------------------
        public override bool Create(IDocument doc)
        {
            TemplateCodeService templateService = ServicesManager.ServicesManagerInstance.GetService(typeof(TemplateCodeService)) as TemplateCodeService;
            // "Document.cs"
            string code = templateService.GetTemplateCode(doc, templateFile);

            IBaseModuleInfo info = PathFinder.GetModuleInfoByName(doc.Namespace.Application, doc.Namespace.Module);
            string fileName = info.GetDocumentPath(doc.Namespace.Document) + Path.GetExtension(templateFile);

            return base.Create(doc, fileName, code);
        }
    }
}
