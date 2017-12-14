using Microarea.TaskBuilderNet.Core.EasyStudioServer.Services;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Interfaces.Model;
using System.IO;

namespace Microarea.TaskBuilderNet.Core.EasyStudioServer.Serializers
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

            IBaseModuleInfo info = PathFinder.GetModuleInfoByName(doc.Namespace.Application, doc.Namespace.Module);
            string docPath = info.GetDocumentPath(doc.Namespace.Document);

            string fileName = docPath + Path.GetExtension(templateHeader);
            if (base.Create(doc, fileName, code))
            {
                // "Document.cpp"
                fileName = docPath + Path.GetExtension(templateSource);
                code = templateService.GetTemplateCode(doc, templateSource);
                return base.Create(doc, fileName, code);
            }

            return false;
        }
    }
}
