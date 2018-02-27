using Microarea.Common.NameSolver;
using TaskBuilderNetCore.Documents.Model.Interfaces;
using TaskBuilderNetCore.EasyStudio.Services;

namespace TaskBuilderNetCore.EasyStudio.Serializers
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

            ModuleInfo info = this.PathFinder.GetModuleInfoByName(doc.NameSpace.Application, doc.NameSpace.Module);
            string fileName = info.GetDocumentPath(doc.NameSpace.Document) + System.IO.Path.GetExtension(templateFile);

            return base.Create(doc, fileName, code);
        }
    }
}
