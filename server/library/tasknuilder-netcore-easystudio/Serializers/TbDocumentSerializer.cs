using TaskBuilderNetCore.Documents.Model.Interfaces;
using TaskBuilderNetCore.EasyStudio.Services;
using Microarea.Common.NameSolver;

namespace TaskBuilderNetCore.EasyStudio.Serializers
{
   //====================================================================
    public class TBDocumentSerializer : BaseDocumentSerializer
    {
        const string templateHeader = "Document.h";
        const string templateSource = "Document.cpp";
        //---------------------------------------------------------------
        public TBDocumentSerializer()
        {
        }

        //---------------------------------------------------------------
        public override bool Create(IDocument doc)
        {
            TemplateCodeService templateService = ServicesManager.ServicesManagerInstance.GetService(typeof(TemplateCodeService)) as TemplateCodeService;
            if (templateService == null)
                return false;

            // "Document.h"
            string code = templateService.GetTemplateCode(doc, templateHeader);

            ModuleInfo info = PathFinder.GetModuleInfoByName(doc.NameSpace.Application, doc.NameSpace.Module);
            string docPath = info.GetDocumentPath(doc.NameSpace.Document);

            string fileName = docPath + System.IO.Path.GetExtension(templateHeader);
            if (base.Create(doc, fileName, code))
            {
                // "Document.cpp"
                fileName = docPath + System.IO.Path.GetExtension(templateSource);
                code = templateService.GetTemplateCode(doc, templateSource);
                return base.Create(doc, fileName, code);
            }

            return false;
        }
    }
}
