using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskBuilderNetCore.Documents.Interfaces;

namespace TaskBuilderNetCore.Documents.Model
{
    public class Extension : IExtension
    {
        Document document;

        public Extension()
        {
        }

        public Document Document
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
