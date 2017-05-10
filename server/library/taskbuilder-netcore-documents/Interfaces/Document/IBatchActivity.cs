using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskBuilderNetCore.Documents.Interfaces
{
    //====================================================================================    
    public interface IBatchActivity
    {
        int Steps { get; }

        void StartBatch();
        void StopBatch();
        void PauseBatch();
        void ResumeBatch();
        void ExecuteBatch(int nStep = 1);
    }
}
