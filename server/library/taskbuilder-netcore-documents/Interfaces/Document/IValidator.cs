using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskBuilderNetCore.Documents.Interfaces
{
    public enum ValidationType { SavingData, OnDemand };

    //====================================================================================    
    public interface IValidator
    {
        ValidationType UsedValidationType { get; }

        bool Validate(IDocument document);
    }
}
