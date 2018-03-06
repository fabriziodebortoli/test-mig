
using TaskBuilderNetCore.Interfaces;

namespace TaskBuilderNetCore.EasyStudio.Interfaces
{
    //====================================================================
    public interface IService
    {
        ISerializer Serializer { get; set; }
        IServiceManager Services { get; set; }
        IDiagnosticProvider Diagnostic { get; set; }
    }
}
