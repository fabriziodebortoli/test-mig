using System;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.EasyStudioServer.Serializers;
using Microarea.TaskBuilderNet.Interfaces.EasyStudioServer;
using Microarea.TaskBuilderNet.Interfaces.Model;
using System.IO;

namespace Microarea.TaskBuilderNet.Core.EasyStudioServer.Services
{
    //====================================================================
    public abstract class BaseDocumentSerializer : Serializer, IDocumentSerializer
    {
        public event EventHandler<SerializerEventArgs> CreatingDocumentSource;
        public event EventHandler<SerializerEventArgs> DocumentSourceCreated;

        //---------------------------------------------------------------
        public BaseDocumentSerializer()
        {
        }

        //---------------------------------------------------------------
        public bool DeclareDocument(IDocumentInfo iDocInfo)
        {
            DocumentInfo docInfo = iDocInfo as DocumentInfo;
            DocumentsObjectInfo docInfos = docInfo.OwnerModule.DocumentObjectsInfo as DocumentsObjectInfo;

            string documentsObjectPath = docInfo.OwnerModule.GetDocumentObjectsPath();
            return docInfos.UnParse(documentsObjectPath);
        }


        //---------------------------------------------------------------
        private void OnCreatingDocumentSource(SerializerEventArgs eventArgs)
        {
            if (CreatingDocumentSource != null)
                CreatingDocumentSource(this, eventArgs);
        }

        //---------------------------------------------------------------
        private void OnDocumentSourceCreated(SerializerEventArgs eventArgs)
        {
            if (DocumentSourceCreated != null)
                DocumentSourceCreated(this, eventArgs);
        }

        //---------------------------------------------------------------
        public virtual bool Create(IDocument doc, string fileName, string defaultCode = null)
        {
            if (!doc.Namespace.IsValid())
                return false;

            SerializerEventArgs eventArgs = new SerializerEventArgs(doc.Namespace);
            eventArgs.Code = defaultCode;
            eventArgs.FileName = fileName;
            OnCreatingDocumentSource(eventArgs);

            string code = eventArgs.Code;
            // TODOBRUNA ad es.
            if (!string.IsNullOrEmpty(code))
            {
                using (FileStream file = new FileStream(eventArgs.FileName, FileMode.Create, FileAccess.Write))
                {
                    StreamWriter streamWriter = new StreamWriter(file);
                    streamWriter.Write(code);
                    streamWriter.Close();
                }
            }

            OnDocumentSourceCreated(eventArgs);
            return true;
        }

        //---------------------------------------------------------------
        public abstract bool Create(IDocument doc);
    }
}
