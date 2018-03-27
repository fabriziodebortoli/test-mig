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
            string code = string.Empty;

            ServicesManager servMng = new ServicesManager();
            TemplateCodeService templateService = servMng.GetService<TemplateCodeService>();

            code = templateService.GetTemplateCode(doc, templateService.SubPathTBApplication, templateHeader);

            ModuleInfo info = PathFinder.GetModuleInfoByName(doc.NameSpace.Application, doc.NameSpace.Module);
            string docPath = info.GetDocumentPath(doc.NameSpace.Document);

            string fileName = docPath + System.IO.Path.GetExtension(templateHeader);
            if (base.Create(doc, fileName, code))
            {
                // "Document.cpp"
                fileName = docPath + System.IO.Path.GetExtension(templateSource);
                code = templateService.GetTemplateCode(doc, templateService.SubPathTBApplication, templateSource);
                return base.Create(doc, fileName, code);
            }

            return false;
        }
    }
}
