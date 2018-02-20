using System;
using System.Collections.Generic;
using System.Text;

namespace TaskBuilderNetCore.Documents.Model.Interfaces
{
    //====================================================================================    
    public interface IDataBag
    {
        bool IsReadOnly { get ; set; }
        bool IsModified { get ; set; }
        bool IsHidden { get ; set; }
    }
}
