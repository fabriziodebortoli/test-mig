using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskBuilderNetCore.Documents.Model.Interfaces
{
    //====================================================================================    
    public interface IDocumentComponent
    {
        IDocument Document { get; set; }
    }
}
