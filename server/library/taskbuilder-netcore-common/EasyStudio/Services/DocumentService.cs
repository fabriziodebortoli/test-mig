using Microarea.Common.Generic;
using Microarea.Common.NameSolver;
using System.ComponentModel;
using TaskBuilderNetCore.Documents.Model.Interfaces;
using TaskBuilderNetCore.EasyStudio.Engine.Serializers;
using TaskBuilderNetCore.EasyStudio.Interfaces;
using TaskBuilderNetCore.Common.CustomAttributes;

namespace TaskBuilderNetCore.EasyStudio.Engine.Services
{
    //====================================================================
    [Name("docSvc"), Description("This service manages document structure info and serialization.")]
    [DefaultSerializer(typeof(DocumentSerializer))]
    public class DocumentService : Component, IService
    {
        BaseDocumentSerializer serializer;
       
        //---------------------------------------------------------------
        private BaseDocumentSerializer DocSerializer
        {
            get
            {
                if (serializer == null)
                    Serializer = DefaultSerializer;

                return serializer;
            }

            set
            {
                serializer = value ;
            }
        }

        //---------------------------------------------------------------
        public ISerializer Serializer
        {
            get
            {
                return DocSerializer;
            }

            set
            {
                if (value is BaseDocumentSerializer)
                    DocSerializer = value as BaseDocumentSerializer;
                else
                    throw (new SerializerException(value, string.Format(Strings.WrongSerializerType, typeof(IDocumentSerializer).Name)));
            }
        }

        //---------------------------------------------------------------
        public DocumentService()
        {
        }

        //---------------------------------------------------------------
        public bool DeclareDocumentFrom(string fromTemplateNamespace, string documentNamespace, string title)
        {
            if (string.IsNullOrEmpty(fromTemplateNamespace) || string.IsNullOrEmpty(documentNamespace))
                return false;

            DocumentInfo documentInfo = CloneDocumentInfo(fromTemplateNamespace, documentNamespace, title);
            if (documentInfo == null)
                return false;

            documentInfo.OwnerModule?.DocumentObjectsInfo.Documents.Add(documentInfo);
            return DocSerializer.DeclareDocument(documentInfo);
        }

        //---------------------------------------------------------------
        public bool DeclareDocument(DocumentInfo documentInfo)
        {
            if (documentInfo == null || documentInfo.OwnerModule == null)
                return false;

            documentInfo?.OwnerModule.DocumentObjectsInfo.Documents.Add(documentInfo);
            return DocSerializer.DeclareDocument(documentInfo);
        }

        //---------------------------------------------------------------
        public bool CreateDocument(IDocument doc, bool withSouruces = false)
        {
            DocumentInfo documentInfo = CreateDocumentInfo(doc.NameSpace.FullNameSpace, ""/*doc.Title*/, false);
            if (documentInfo == null)
                return false;

            bool declared = DeclareDocument(documentInfo);
            if (!declared)
                return false;

            return withSouruces ? DocSerializer.Create(doc) : true;
        }

        //---------------------------------------------------------------
        private DocumentInfo CreateDocumentInfo(string documentNamespace, string title, bool batch)
        {
            NameSpace newNs = new NameSpace(documentNamespace);
            ModuleInfo moduleInfo = DocSerializer.PathFinder.GetModuleInfoByName(newNs.Application, newNs.Module);
            if (moduleInfo == null)
                return null;

            DocumentInfo documentInfo = moduleInfo.GetDocumentInfoByNameSpace(newNs) as DocumentInfo;
            if (documentInfo == null)
                documentInfo = new DocumentInfo(moduleInfo, newNs, title, title, "", "");

            documentInfo.IsBatch = batch;
            documentInfo.IsDataEntry = !documentInfo.IsBatch;
            return documentInfo;
        }

        //---------------------------------------------------------------
        private DocumentInfo CloneDocumentInfo(string fromTemplateNamespace, string documentNamespace, string title)
        {
            NameSpace newNs = new NameSpace(documentNamespace);
            ModuleInfo moduleInfo = DocSerializer.PathFinder.GetModuleInfoByName(newNs.Application, newNs.Module);
            if (moduleInfo == null)
                return null;

            NameSpace originalNs = new NameSpace(fromTemplateNamespace);
            DocumentInfo originalDocumentInfo = DocSerializer.PathFinder.GetDocumentInfo(originalNs) as DocumentInfo;
            if (originalDocumentInfo == null)
                return null;

            DocumentInfo documentInfo = originalDocumentInfo.Clone() as DocumentInfo;
            documentInfo.NameSpace = newNs;
            documentInfo.OwnerModule = moduleInfo;
            documentInfo.TemplateNamespace = originalNs;
            documentInfo.Title = title;
            documentInfo.IsDynamic = false;

            return documentInfo;
        }
    }
}
