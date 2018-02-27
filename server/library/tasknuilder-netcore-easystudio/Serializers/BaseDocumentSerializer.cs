using System;
using TaskBuilderNetCore.Documents.Model.Interfaces;
using TaskBuilderNetCore.EasyStudio.Interfaces;
using TaskBuilderNetCore.Interfaces;
using Microarea.Common.NameSolver;

namespace TaskBuilderNetCore.EasyStudio.Serializers
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
            if (!doc.NameSpace.IsValid())
                return false;

            SerializerEventArgs eventArgs = new SerializerEventArgs(doc.NameSpace);
            eventArgs.Code = defaultCode;
            eventArgs.FileName = fileName;
            OnCreatingDocumentSource(eventArgs);

            string code = eventArgs.Code;
        /*    // TODOBRUNA sostituire pathfinder ad es. 
            if (!string.IsNullOrEmpty(code))
            {
                using (FileStream file = new FileStream(eventArgs.FileName, FileMode.Create, FileAccess.Write))
                {
                    StreamWriter streamWriter = new StreamWriter(file);
                    streamWriter.Write(code);
                    streamWriter.Close();
                }
            }
            */
            OnDocumentSourceCreated(eventArgs);
            return true;
        }

        //---------------------------------------------------------------
        public abstract bool Create(IDocument doc);
    }
}
