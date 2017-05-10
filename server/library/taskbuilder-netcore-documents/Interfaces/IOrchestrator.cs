using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskBuilderNetCore.Documents.Interfaces
{
    //====================================================================================    
    public interface IOrchestrator
    {
        ILoader Loader  { get; }
        IRecycler Recycler { get; }
        ILicenceConnector LicenceConnector  { get; }
        IJsonSerializer JsonSerializer { get; }
        IWebConnector WebConnector { get; }
        IUIController UIController { get; }


        void CloseDocument(IDocument document);
    }
}
