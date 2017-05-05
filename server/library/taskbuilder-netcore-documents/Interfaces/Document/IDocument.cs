using System;
using TaskBuilderNetCore.Interfaces;

namespace TaskBuilderNetCore.Documents.Interfaces
{
    public interface IDocument
    {
        INameSpace NameSpace { get; }
    }
}
