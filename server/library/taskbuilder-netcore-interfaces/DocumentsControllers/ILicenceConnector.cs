using TaskBuilderNetCore.Interfaces;

namespace TaskBuilderNetCore.Documents.Controllers.Interfaces
{
    //====================================================================================    
    public interface ILicenceConnector
    {
        bool IsActivated(INameSpace nameSpace);
        bool IsActivated(string activation);
        bool IsActivated(string application, string moduleOrFunctionality);
    }
}