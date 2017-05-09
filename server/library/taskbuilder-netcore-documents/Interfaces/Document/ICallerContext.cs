using TaskBuilderNetCore.Interfaces;

namespace TaskBuilderNetCore.Documents.Interfaces
{
    //====================================================================================    
    public interface ICallerContext
    {
        INameSpace NameSpace { get; }
        string AuthToken { get; }
        string Company { get; }

        bool IsSameIdentity(ICallerContext context);
    }
}
