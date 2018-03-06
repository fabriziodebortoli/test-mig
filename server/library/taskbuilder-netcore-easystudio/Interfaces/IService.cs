
using TaskBuilderNetCore.Interfaces;

namespace TaskBuilderNetCore.EasyStudio.Interfaces
{
    //====================================================================
    public interface IService
    {
        string Name { get; }
        string Description { get; }
        ISerializer Serializer { get; set; }
        IServiceManager Services { get; set; }
        IDiagnosticProvider Diagnostic { get; set; }
        
    }
}
