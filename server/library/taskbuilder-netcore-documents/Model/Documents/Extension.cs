using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskBuilderNetCore.Documents.Interfaces;

namespace TaskBuilderNetCore.Documents.Model
{
    //====================================================================================    
    public class Extension : IExtension
    {
        IDocument document;

        //-----------------------------------------------------------------------------------------------------
        public Extension()
        {
        }

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
