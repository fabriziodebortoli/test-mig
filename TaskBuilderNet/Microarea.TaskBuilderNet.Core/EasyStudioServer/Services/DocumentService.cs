using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.EasyStudioServer.Serializers;
using System.ComponentModel;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Interfaces.EasyStudioServer;
using System;
using System.Collections.Generic;
using Microarea.TaskBuilderNet.Interfaces.Model;

namespace Microarea.TaskBuilderNet.Core.EasyStudioServer.Services
{
    //====================================================================
    [DisplayName("docSvc"), Description("This service manages document structure info and serialization.")]
    [DefaultSerializer(typeof(DocumentSerializer))]
    public class DocumentService : Component, IService
    {
        IDocumentSerializer serializer;
       
        //---------------------------------------------------------------
        private IDocumentSerializer DocSerializer
        {
            get
            {
                if (serializer == null)
                    serializer = DefaultSerializer as IDocumentSerializer;

                return serializer;
            }

            set
            {
                serializer = value;
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
                if (value is IDocumentSerializer)
                    DocSerializer = (IDocumentSerializer) value;
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
            DocumentInfo documentInfo = CreateDocumentInfo(doc.Namespace.FullNameSpace, doc.Title, doc.Batch);
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

            IBaseModuleInfo moduleInfo = Serializer.PathFinder.GetModuleInfoByName(newNs.Application, newNs.Module);
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

            IBaseModuleInfo moduleInfo = Serializer.PathFinder.GetModuleInfoByName(newNs.Application, newNs.Module);
            if (moduleInfo == null)
                return null;

            NameSpace originalNs = new NameSpace(fromTemplateNamespace);
            DocumentInfo originalDocumentInfo = Serializer.PathFinder.GetDocumentInfo(originalNs) as DocumentInfo;
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
