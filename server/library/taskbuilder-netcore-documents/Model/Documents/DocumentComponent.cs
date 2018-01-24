using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskBuilderNetCore.Documents.Model.Interfaces;

namespace TaskBuilderNetCore.Documents.Model
{
    //====================================================================================    
    public class DocumentComponent : Component, IDocumentComponent
    {
        IDocument document;

        //-----------------------------------------------------------------------------------------------------
        public DocumentComponent()
        {
        }

        //-----------------------------------------------------------------------------------------------------
        public bool CanBeLoaded(IDocument document)
        {
            return true;
        }
        [JsonIgnore]
        //-----------------------------------------------------------------------------------------------------
        public IDocument Document
        {
            get
            {
                return document;
            }

            set
            {
                document = value;
            }
        }
    }
}
