using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskBuilderNetCore.Interfaces;

namespace TaskBuilderNetCore.Documents.Interfaces
{
    public interface ILoader
    {
        Type GetDocument(INameSpace nameSpace);
    }
}
