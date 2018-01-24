
using TaskBuilderNetCore.Documents.Diagnostic;

namespace TaskBuilderNetCore.Documents.Controllers
{
    //====================================================================================    
    public class LoaderMessages
    {
        public static readonly Message ClassNotFound = new Message(1, LoaderMessagesStrings.ClassNotFound);
        public static readonly Message DocumentNotDeclared = new Message(2, LoaderMessagesStrings.DocumentNotDeclared);
        public static readonly Message DocumentNotLoaded = new Message(3, LoaderMessagesStrings.DocumentNotLoaded);
        public static readonly Message LoadAssemblyError = new Message(4, LoaderMessagesStrings.LoadAssemblyError);
        public static readonly Message AssemblyNotLoaded = new Message(5, LoaderMessagesStrings.AssemblyNotLoaded);
    }
}
