using TaskBuilderNetCore.Interfaces;

namespace TaskBuilderNetCore.Documents.Interfaces
{
    public interface ILicenceConnector
    {
        bool IsActivated(INameSpace nameSpace);
    }
}