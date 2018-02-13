using System.Collections.ObjectModel;
using TaskBuilderNetCore.Documents.Model.Interfaces;
namespace TaskBuilderNetCore.Documents.Controllers.Interfaces
{
    //====================================================================================    
    public interface IOrchestrator
    {
        ObservableCollection<IDocument> Documents { get; }
        ObservableCollection<IComponent> Components { get; }

        ILoader Loader  { get; }
        IRecycler Recycler { get; }
        ILicenceConnector LicenceConnector  { get; }
        IStateSerializer StateSerializer { get; }
        IUIController UIController { get; }
        ILogger Logger { get; }
    }
}
